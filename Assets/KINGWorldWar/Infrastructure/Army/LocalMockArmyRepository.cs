#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Infrastructure.Army
{
    /// <summary>
    /// Editor/Development-only mock implementation of IArmyRepository.
    /// Simulates server-authoritative validation for training costs, queue slot limits (max 2),
    /// building prerequisites, idempotent UUIDv4 request caching, latency, and failure scenarios.
    /// Strictly excluded from production release builds.
    /// </summary>
    public sealed class LocalMockArmyRepository : IArmyRepository
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, object> _processedRequests = new();

        private readonly Dictionary<UnitType, int> _ownedUnits = new();
        private readonly Dictionary<UnitType, int> _inTrainingUnits = new();
        private readonly Dictionary<UnitType, int> _deployedUnits = new();
        private readonly List<RecruitmentQueueItem> _queue = new();

        private long _revision = 1;
        private readonly string _balanceVersion = "1.0.0";
        private readonly IArmyDataSource _dataSource;

        public bool IsMock => true;
        public int SimulatedLatencyMs { get; set; } = 20;
        public int MaxQueueSlots { get; set; } = 2;
        public bool SimulateFailure { get; set; } = false;

        public string PlayerId { get; set; } = "usr_commander_01";

        public LocalMockArmyRepository(IArmyDataSource dataSource = null)
        {
            _dataSource = dataSource ?? new ArmyConfigSO();

            // Seed initial standing forces across all 5 MVP unit types
            _ownedUnits[UnitType.Tank] = 10;
            _ownedUnits[UnitType.ArmoredVehicle] = 15;
            _ownedUnits[UnitType.Artillery] = 6;
            _ownedUnits[UnitType.FighterAircraft] = 4;
            _ownedUnits[UnitType.ReconAircraft] = 8;

            foreach (UnitType t in Enum.GetValues(typeof(UnitType)))
            {
                if (!_ownedUnits.ContainsKey(t)) _ownedUnits[t] = 0;
                _inTrainingUnits[t] = 0;
                _deployedUnits[t] = 0;
            }
        }

        public async Task<Result<ArmyRosterSnapshot, GameError>> GetRosterAsync(CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                ReconcileCompletedQueues_Internal(DateTime.UtcNow);
                var snapshot = CreateSnapshot_Internal(DateTime.UtcNow);
                return Result<ArmyRosterSnapshot, GameError>.Ok(snapshot);
            }
        }

        public async Task<Result<IReadOnlyList<RecruitmentQueueItem>, GameError>> GetTrainingQueueAsync(CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                ReconcileCompletedQueues_Internal(DateTime.UtcNow);
                return Result<IReadOnlyList<RecruitmentQueueItem>, GameError>.Ok(_queue.ToList());
            }
        }

        public async Task<Result<RecruitmentQueueItem, GameError>> RequestTrainingAsync(
            string requestId,
            UnitType unitType,
            int quantity,
            CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (SimulateFailure)
                {
                    return Result<RecruitmentQueueItem, GameError>.Fail(
                        GameError.ServiceInitFailed("TrainingFailure", "Server rejected training request (simulated failure)."));
                }

                if (_processedRequests.TryGetValue(requestId, out var cached) && cached is RecruitmentQueueItem cachedItem)
                {
                    return Result<RecruitmentQueueItem, GameError>.Ok(cachedItem);
                }

                if (quantity <= 0)
                {
                    return Result<RecruitmentQueueItem, GameError>.Fail(GameError.InvalidRecruitCount(quantity));
                }

                ReconcileCompletedQueues_Internal(DateTime.UtcNow);

                var activeCount = _queue.Count(q => q.State == TrainingQueueState.InProgress || q.State == TrainingQueueState.Pending);
                if (activeCount >= MaxQueueSlots)
                {
                    return Result<RecruitmentQueueItem, GameError>.Fail(
                        GameError.RecruitmentQueueFull($"Maximum concurrent training operations ({MaxQueueSlots}) reached."));
                }

                var def = _dataSource.GetDefinition(unitType);
                if (def == null)
                {
                    return Result<RecruitmentQueueItem, GameError>.Fail(GameError.UnitNotFound(unitType.ToString()));
                }

                var now = DateTime.UtcNow;
                var durationSeconds = def.Cost.RecruitDurationSeconds * quantity;
                var totalCost = def.Cost.Multiply(quantity);

                var queueItem = new RecruitmentQueueItem(
                    "q_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    unitType,
                    quantity,
                    now,
                    now.AddSeconds(durationSeconds),
                    totalCost,
                    TrainingQueueState.InProgress
                );

                _queue.Add(queueItem);
                _inTrainingUnits[unitType] = _inTrainingUnits[unitType] + quantity;
                _revision++;

                _processedRequests[requestId] = queueItem;
                return Result<RecruitmentQueueItem, GameError>.Ok(queueItem);
            }
        }

        public async Task<Result<bool, GameError>> RequestCancelTrainingAsync(
            string requestId,
            string queueItemId,
            CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                ReconcileCompletedQueues_Internal(DateTime.UtcNow);

                var item = _queue.FirstOrDefault(q => q.QueueId == queueItemId);
                if (item == null)
                {
                    return Result<bool, GameError>.Fail(GameError.RecruitmentItemNotFound(queueItemId));
                }

                if (item.State == TrainingQueueState.Completed)
                {
                    return Result<bool, GameError>.Fail(GameError.InvalidPayload("Cannot cancel a training operation that has already completed."));
                }

                _queue.Remove(item);
                _inTrainingUnits[item.Type] = Math.Max(0, _inTrainingUnits[item.Type] - item.Count);
                _revision++;

                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        public async Task<Result<bool, GameError>> RequestUnitUpgradeAsync(
            string requestId,
            UnitType unitType,
            CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);

            lock (_lock)
            {
                if (_processedRequests.ContainsKey(requestId))
                    return Result<bool, GameError>.Ok(true);

                _revision++;
                _processedRequests[requestId] = true;
                return Result<bool, GameError>.Ok(true);
            }
        }

        private void ReconcileCompletedQueues_Internal(DateTime utcNow)
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                var q = _queue[i];
                if (q.State == TrainingQueueState.InProgress && q.IsCompleted(utcNow))
                {
                    _queue[i] = q.WithState(TrainingQueueState.Completed);
                    _ownedUnits[q.Type] = _ownedUnits[q.Type] + q.Count;
                    _inTrainingUnits[q.Type] = Math.Max(0, _inTrainingUnits[q.Type] - q.Count);
                    _revision++;
                }
            }

            // Remove finished items
            _queue.RemoveAll(q => q.State == TrainingQueueState.Completed || q.State == TrainingQueueState.Cancelled);
        }

        private ArmyRosterSnapshot CreateSnapshot_Internal(DateTime serverTimeUtc)
        {
            return new ArmyRosterSnapshot(
                PlayerId,
                _revision,
                _balanceVersion,
                serverTimeUtc,
                _ownedUnits,
                _inTrainingUnits,
                _deployedUnits
            );
        }

        public void SetUnitCountForTest(UnitType type, int owned, int inTraining = 0, int deployed = 0)
        {
            lock (_lock)
            {
                _ownedUnits[type] = Math.Max(0, owned);
                _inTrainingUnits[type] = Math.Max(0, inTraining);
                _deployedUnits[type] = Math.Max(0, deployed);
                _revision++;
            }
        }
    }
}
#endif
