using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Logging;

namespace KingWorldWar.Infrastructure.Army
{
    /// <summary>
    /// Service coordinating military unit recruitment, queue progression, and army roster.
    /// Non-authoritative local client simulation; maps to server-authoritative Nakama RPCs in Phase 8.
    /// </summary>
    public sealed class ArmyService : IArmyService
    {
        private const string Tag = "ArmyService";

        public IArmyDataSource DataSource { get; }
        public IArmyStateRepository StateRepository { get; }
        public IBaseStateRepository BaseRepository { get; }

        private readonly ILog _log;

        public event Action<RecruitmentQueueItem> OnRecruitmentStarted;
        public event Action<string> OnRecruitmentCancelled;
        public event Action<UnitType, int> OnRecruitmentCompleted;
        public event Action OnRosterUpdated;

        public ArmyService(
            IArmyDataSource dataSource,
            IArmyStateRepository stateRepository,
            IBaseStateRepository baseRepository = null,
            ILog log = null)
        {
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            StateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
            BaseRepository = baseRepository;
            _log = log ?? GameLog.Default;
        }

        public ArmyRoster GetRoster() => StateRepository.GetRoster();

        public int GetTotalCombatPower() => GetRoster().CalculateTotalCombatPower(DataSource);

        public IReadOnlyList<RecruitmentQueueItem> GetActiveQueues() => StateRepository.GetActiveQueues();

        public bool CheckPrerequisites(UnitType type, out string reason)
        {
            if (!DataSource.TryGetDefinition(type, out var def))
            {
                reason = "Unknown unit archetype.";
                return false;
            }

            // If BaseRepository is provided, verify required building level
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

        public Task<Result<RecruitmentQueueItem, GameError>> RequestRecruitmentAsync(
            UnitType type,
            int count,
            CancellationToken ct = default)
        {
            if (count <= 0)
            {
                return Task.FromResult(Result<RecruitmentQueueItem, GameError>.Fail(
                    GameError.InvalidRecruitCount(count)));
            }

            if (!DataSource.TryGetDefinition(type, out var def))
            {
                return Task.FromResult(Result<RecruitmentQueueItem, GameError>.Fail(
                    GameError.UnitNotFound(type.ToString())));
            }

            if (!CheckPrerequisites(type, out var reason))
            {
                return Task.FromResult(Result<RecruitmentQueueItem, GameError>.Fail(
                    GameError.UnitPrerequisiteNotMet(reason)));
            }

            var active = StateRepository.GetActiveQueues();
            if (active.Count >= StateRepository.MaxConcurrentQueues)
            {
                return Task.FromResult(Result<RecruitmentQueueItem, GameError>.Fail(
                    GameError.RecruitmentQueueFull($"Maximum {StateRepository.MaxConcurrentQueues} queues active.")));
            }

            // Compute total cost and duration
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
                totalCost: totalCost);

            StateRepository.EnqueueRecruitment(queueItem);
            _log.Info(Tag, $"Recruitment started: {type} x{count} (Duration: {totalCost.RecruitDurationSeconds}s)");

            OnRecruitmentStarted?.Invoke(queueItem);
            return Task.FromResult(Result<RecruitmentQueueItem, GameError>.Ok(queueItem));
        }

        public Task<Result<GameError>> CancelRecruitmentAsync(
            string queueId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(queueId))
            {
                return Task.FromResult(Result<GameError>.Fail(
                    GameError.RecruitmentItemNotFound(queueId)));
            }

            if (!StateRepository.TryGetQueueItem(queueId, out var item))
            {
                return Task.FromResult(Result<GameError>.Fail(
                    GameError.RecruitmentItemNotFound(queueId)));
            }

            StateRepository.RemoveQueueItem(queueId);
            _log.Info(Tag, $"Recruitment cancelled: {item.Type} x{item.Count} (QueueId: {queueId})");

            OnRecruitmentCancelled?.Invoke(queueId);
            return Task.FromResult(Result<GameError>.Ok());
        }

        public Result<int, GameError> CheckCompletedRecruitments(DateTime utcNow)
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
                OnRosterUpdated?.Invoke();
            }

            return Result<int, GameError>.Ok(completedCount);
        }
    }
}
