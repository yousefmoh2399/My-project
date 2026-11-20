using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Logging;

namespace KingWorldWar.Infrastructure.Army
{
    /// <summary>
    /// Service orchestrating military unit recruitment, queue progression, standing roster, and upgrades.
    /// Strictly enforces Untrusted Client principles by routing mutations through IArmyRepository.
    /// Provides backward compatibility for local state simulation when IArmyStateRepository is present.
    /// </summary>
    public sealed class ArmyService : IArmyService
    {
        private const string Tag = "ArmyService";

        public IArmyDataSource DataSource { get; }
        public IArmyRepository Repository { get; }
        public IArmyStateRepository StateRepository { get; }
        public IBaseStateRepository BaseRepository { get; }

        public ArmyRosterSnapshot CurrentSnapshot { get; private set; }

        private readonly List<RecruitmentQueueItem> _activeQueues = new List<RecruitmentQueueItem>();
        public IReadOnlyList<RecruitmentQueueItem> ActiveQueues => _activeQueues.ToList();
        public int MaxConcurrentQueues => StateRepository != null ? StateRepository.MaxConcurrentQueues : 2;

        private readonly ILog _log;

        public event Action<RecruitmentQueueItem> OnRecruitmentStarted;
        public event Action<string> OnRecruitmentCancelled;
        public event Action<UnitType, int> OnRecruitmentCompleted;
        public event Action OnRosterUpdated;
        public event Action<IReadOnlyList<RecruitmentQueueItem>> OnQueueUpdated;

        public ArmyService(
            IArmyDataSource dataSource,
            IArmyStateRepository stateRepository,
            IBaseStateRepository baseRepository = null,
            ILog log = null)
            : this(dataSource, null, stateRepository, baseRepository, log)
        {
        }

        public ArmyService(
            IArmyDataSource dataSource,
            IArmyRepository repository,
            IBaseStateRepository baseRepository = null,
            ILog log = null)
            : this(dataSource, repository, null, baseRepository, log)
        {
        }

        public ArmyService(
            IArmyDataSource dataSource,
            IArmyRepository repository,
            IArmyStateRepository stateRepository,
            IBaseStateRepository baseRepository = null,
            ILog log = null)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            Repository = repository;
            StateRepository = stateRepository;
            BaseRepository = baseRepository;
            _log = log ?? GameLog.Default;

            // If neither is supplied, instantiate default mock repository for development
            if (Repository == null && StateRepository == null)
            {
#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
                Repository = new LocalMockArmyRepository(DataSource);
#endif
            }

            // Initialize initial snapshot
            InitializeBaselineSnapshot();
        }

        private void InitializeBaselineSnapshot()
        {
            if (StateRepository != null)
            {
                var roster = StateRepository.GetRoster();
                CurrentSnapshot = roster.ToSnapshot();
                _activeQueues.Clear();
                _activeQueues.AddRange(StateRepository.GetActiveQueues());
            }
            else if (Repository != null)
            {
                var dict = new Dictionary<UnitType, int>();
                foreach (UnitType t in Enum.GetValues(typeof(UnitType)))
                {
                    dict[t] = 0;
                }
                CurrentSnapshot = new ArmyRosterSnapshot(
                    "player_local",
                    1,
                    "1.0.0",
                    DateTime.UtcNow,
                    dict,
                    new Dictionary<UnitType, int>(),
                    new Dictionary<UnitType, int>()
                );
            }
        }

        public ArmyRoster GetRoster()
        {
            if (CurrentSnapshot != null)
            {
                return ArmyRoster.FromSnapshot(CurrentSnapshot);
            }

            if (StateRepository != null)
            {
                return StateRepository.GetRoster();
            }

            return new ArmyRoster();
        }

        public int GetTotalCombatPower()
        {
            if (CurrentSnapshot != null)
            {
                return CurrentSnapshot.CalculateTotalCombatPower(DataSource);
            }
            return GetRoster().CalculateTotalCombatPower(DataSource);
        }

        public int GetTotalForcesCount()
        {
            if (CurrentSnapshot != null)
            {
                return CurrentSnapshot.TotalForcesCount;
            }
            return GetRoster().TotalTroops;
        }

        public IReadOnlyList<RecruitmentQueueItem> GetActiveQueues()
        {
            if (StateRepository != null && Repository == null)
            {
                return StateRepository.GetActiveQueues();
            }
            return _activeQueues.ToList();
        }

        public bool CheckPrerequisites(UnitType type, out string reason)
        {
            if (!DataSource.TryGetDefinition(type, out var def))
            {
                reason = "Unknown unit archetype.";
                return false;
            }

            if (BaseRepository != null)
            {
                var buildings = BaseRepository.GetAllBuildings();
                BuildingRuntimeState highestReqBuilding = null;

                for (var i = 0; i < buildings.Count; i++)
                {
                    if (buildings[i].Type == def.RequiredBuildingType)
                    {
                        if (highestReqBuilding == null || buildings[i].CurrentLevel > highestReqBuilding.CurrentLevel)
                        {
                            highestReqBuilding = buildings[i];
                        }
                    }
                }

                if (highestReqBuilding == null)
                {
                    reason = $"Requires {def.RequiredBuildingType} structure.";
                    return false;
                }

                if (highestReqBuilding.CurrentLevel < def.RequiredBuildingLevel)
                {
                    reason = $"Requires {def.RequiredBuildingType} Level {def.RequiredBuildingLevel} (Current: Level {highestReqBuilding.CurrentLevel}).";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        public async Task<Result<ArmyRosterSnapshot, GameError>> RefreshRosterAsync(CancellationToken ct = default)
        {
            if (Repository != null)
            {
                var rosterRes = await Repository.GetRosterAsync(ct).ConfigureAwait(false);
                if (rosterRes.IsFailure)
                {
                    return Result<ArmyRosterSnapshot, GameError>.Fail(rosterRes.Error);
                }

                CurrentSnapshot = rosterRes.Value;

                var queueRes = await Repository.GetTrainingQueueAsync(ct).ConfigureAwait(false);
                if (queueRes.IsSuccess)
                {
                    _activeQueues.Clear();
                    _activeQueues.AddRange(queueRes.Value);
                    OnQueueUpdated?.Invoke(_activeQueues.ToList());
                }

                if (StateRepository != null)
                {
                    StateRepository.UpdateRoster(ArmyRoster.FromSnapshot(CurrentSnapshot));
                }

                OnRosterUpdated?.Invoke();
                return Result<ArmyRosterSnapshot, GameError>.Ok(CurrentSnapshot);
            }

            if (StateRepository != null)
            {
                var roster = StateRepository.GetRoster();
                CurrentSnapshot = roster.ToSnapshot();
                _activeQueues.Clear();
                _activeQueues.AddRange(StateRepository.GetActiveQueues());

                OnQueueUpdated?.Invoke(_activeQueues.ToList());
                OnRosterUpdated?.Invoke();
                return Result<ArmyRosterSnapshot, GameError>.Ok(CurrentSnapshot);
            }

            return Result<ArmyRosterSnapshot, GameError>.Fail(
                GameError.ServiceInitFailed("NoRepository", "No army repository configured."));
        }

        public async Task<Result<RecruitmentQueueItem, GameError>> RequestRecruitmentAsync(
            UnitType type,
            int count,
            CancellationToken ct = default)
        {
            if (count <= 0)
            {
                return Result<RecruitmentQueueItem, GameError>.Fail(
                    GameError.InvalidRecruitCount(count));
            }

            if (!DataSource.TryGetDefinition(type, out var def))
            {
                return Result<RecruitmentQueueItem, GameError>.Fail(
                    GameError.UnitNotFound(type.ToString()));
            }

            if (!CheckPrerequisites(type, out var reason))
            {
                return Result<RecruitmentQueueItem, GameError>.Fail(
                    GameError.UnitPrerequisiteNotMet(reason));
            }

            if (Repository != null)
            {
                var requestId = Guid.NewGuid().ToString("D");
                var result = await Repository.RequestTrainingAsync(requestId, type, count, ct).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    return result;
                }

                var item = result.Value;
                _log.Info(Tag, $"Recruitment started via repository: {type} x{count} (QueueId: {item.QueueId})");

                await RefreshRosterAsync(ct).ConfigureAwait(false);

                OnRecruitmentStarted?.Invoke(item);
                return Result<RecruitmentQueueItem, GameError>.Ok(item);
            }

            if (StateRepository != null)
            {
                var active = StateRepository.GetActiveQueues();
                if (active.Count >= StateRepository.MaxConcurrentQueues)
                {
                    return Result<RecruitmentQueueItem, GameError>.Fail(
                        GameError.RecruitmentQueueFull($"Maximum {StateRepository.MaxConcurrentQueues} queues active."));
                }

                var totalCost = def.Cost.Multiply(count);
                var nowUtc = DateTime.UtcNow;
                var duration = TimeSpan.FromSeconds(totalCost.RecruitDurationSeconds);
                var endTimeUtc = nowUtc.Add(duration);

                var queueId = $"rec_{type}_{Guid.NewGuid():N}".Substring(0, 16);
                var queueItem = new RecruitmentQueueItem(
                    queueId: queueId,
                    type: type,
                    count: count,
                    startTimeUtc: nowUtc,
                    endTimeUtc: endTimeUtc,
                    totalCost: totalCost,
                    state: TrainingQueueState.InProgress);

                StateRepository.EnqueueRecruitment(queueItem);
                _activeQueues.Add(queueItem);
                CurrentSnapshot = StateRepository.GetRoster().ToSnapshot();

                _log.Info(Tag, $"Recruitment started locally: {type} x{count} (Duration: {totalCost.RecruitDurationSeconds}s)");

                OnRecruitmentStarted?.Invoke(queueItem);
                OnQueueUpdated?.Invoke(_activeQueues.ToList());
                return Result<RecruitmentQueueItem, GameError>.Ok(queueItem);
            }

            return Result<RecruitmentQueueItem, GameError>.Fail(
                GameError.ServiceInitFailed("NoRepository", "No army repository configured."));
        }

        public async Task<Result<bool, GameError>> CancelRecruitmentAsync(
            string queueId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(queueId))
            {
                return Result<bool, GameError>.Fail(
                    GameError.RecruitmentItemNotFound(queueId));
            }

            if (Repository != null)
            {
                var requestId = Guid.NewGuid().ToString("D");
                var result = await Repository.RequestCancelTrainingAsync(requestId, queueId, ct).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    return result;
                }

                _log.Info(Tag, $"Recruitment cancelled via repository: QueueId={queueId}");
                await RefreshRosterAsync(ct).ConfigureAwait(false);

                OnRecruitmentCancelled?.Invoke(queueId);
                return Result<bool, GameError>.Ok(true);
            }

            if (StateRepository != null)
            {
                if (!StateRepository.TryGetQueueItem(queueId, out var item))
                {
                    return Result<bool, GameError>.Fail(
                        GameError.RecruitmentItemNotFound(queueId));
                }

                StateRepository.RemoveQueueItem(queueId);
                _activeQueues.RemoveAll(q => q.QueueId == queueId);
                CurrentSnapshot = StateRepository.GetRoster().ToSnapshot();

                _log.Info(Tag, $"Recruitment cancelled locally: {item.Type} x{item.Count} (QueueId: {queueId})");

                OnRecruitmentCancelled?.Invoke(queueId);
                OnQueueUpdated?.Invoke(_activeQueues.ToList());
                return Result<bool, GameError>.Ok(true);
            }

            return Result<bool, GameError>.Fail(
                GameError.ServiceInitFailed("NoRepository", "No army repository configured."));
        }

        public async Task<Result<bool, GameError>> RequestUnitUpgradeAsync(
            UnitType type,
            CancellationToken ct = default)
        {
            if (!DataSource.TryGetDefinition(type, out var def))
            {
                return Result<bool, GameError>.Fail(GameError.UnitNotFound(type.ToString()));
            }

            if (Repository != null)
            {
                var requestId = Guid.NewGuid().ToString("D");
                var result = await Repository.RequestUnitUpgradeAsync(requestId, type, ct).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    return result;
                }

                await RefreshRosterAsync(ct).ConfigureAwait(false);
                return Result<bool, GameError>.Ok(true);
            }

            return Result<bool, GameError>.Ok(true);
        }

        public Result<int, GameError> CheckCompletedRecruitments(DateTime utcNow)
        {
            if (StateRepository != null)
            {
                var active = StateRepository.GetActiveQueues();
                var completedCount = 0;
                var roster = StateRepository.GetRoster();

                for (var i = 0; i < active.Count; i++)
                {
                    var item = active[i];
                    if (item.IsCompleted(utcNow))
                    {
                        StateRepository.RemoveQueueItem(item.QueueId);
                        roster.AddUnits(item.Type, item.Count);
                        completedCount++;

                        _log.Info(Tag, $"Recruitment completed: {item.Type} x{item.Count}");
                        OnRecruitmentCompleted?.Invoke(item.Type, item.Count);
                    }
                }

                if (completedCount > 0)
                {
                    StateRepository.UpdateRoster(roster);
                    CurrentSnapshot = roster.ToSnapshot();
                    _activeQueues.Clear();
                    _activeQueues.AddRange(StateRepository.GetActiveQueues());

                    OnRosterUpdated?.Invoke();
                    OnQueueUpdated?.Invoke(_activeQueues.ToList());
                }

                return Result<int, GameError>.Ok(completedCount);
            }

            if (Repository != null)
            {
                int completedCount = 0;
                for (int i = 0; i < _activeQueues.Count; i++)
                {
                    var item = _activeQueues[i];
                    if (item.IsCompleted(utcNow))
                    {
                        completedCount++;
                        OnRecruitmentCompleted?.Invoke(item.Type, item.Count);
                    }
                }

                if (completedCount > 0)
                {
                    _ = RefreshRosterAsync(CancellationToken.None);
                }

                return Result<int, GameError>.Ok(completedCount);
            }

            return Result<int, GameError>.Ok(0);
        }
    }
}
