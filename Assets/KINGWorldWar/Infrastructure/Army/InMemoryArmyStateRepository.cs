using System;
using System.Collections.Generic;
using KingWorldWar.Core.Army;

namespace KingWorldWar.Infrastructure.Army
{
    /// <summary>
    /// In-memory implementation of IArmyStateRepository for Phase 6.
    /// Thread-safe storage for active army roster and recruitment queues.
    /// </summary>
    public sealed class InMemoryArmyStateRepository : IArmyStateRepository
    {
        private readonly object _lock = new object();
        private ArmyRoster _roster;
        private readonly List<RecruitmentQueueItem> _activeQueues = new List<RecruitmentQueueItem>();

        public int MaxConcurrentQueues { get; }

        public InMemoryArmyStateRepository(int maxConcurrentQueues = 2, ArmyRoster initialRoster = null)
        {
            MaxConcurrentQueues = Math.Max(1, maxConcurrentQueues);

            if (initialRoster != null)
            {
                _roster = initialRoster.Clone();
            }
            else
            {
                // Starting army baseline for tactical testing
                _roster = new ArmyRoster(new Dictionary<UnitType, int>
                {
                    { UnitType.Infantry, 100 },
                    { UnitType.ReconVehicle, 10 },
                    { UnitType.Tank, 5 },
                    { UnitType.Artillery, 0 }
                });
            }
        }

        public ArmyRoster GetRoster()
        {
            lock (_lock)
            {
                return _roster.Clone();
            }
        }

        public void UpdateRoster(ArmyRoster roster)
        {
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            lock (_lock)
            {
                _roster = roster.Clone();
            }
        }

        public IReadOnlyList<RecruitmentQueueItem> GetActiveQueues()
        {
            lock (_lock)
            {
                return new List<RecruitmentQueueItem>(_activeQueues);
            }
        }

        public bool TryGetQueueItem(string queueId, out RecruitmentQueueItem item)
        {
            lock (_lock)
            {
                for (var i = 0; i < _activeQueues.Count; i++)
                {
                    if (string.Equals(_activeQueues[i].QueueId, queueId, StringComparison.OrdinalIgnoreCase))
                    {
                        item = _activeQueues[i];
                        return true;
                    }
                }
                item = null;
                return false;
            }
        }

        public void EnqueueRecruitment(RecruitmentQueueItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            lock (_lock)
            {
                _activeQueues.Add(item);
            }
        }

        public bool RemoveQueueItem(string queueId)
        {
            lock (_lock)
            {
                for (var i = 0; i < _activeQueues.Count; i++)
                {
                    if (string.Equals(_activeQueues[i].QueueId, queueId, StringComparison.OrdinalIgnoreCase))
                    {
                        _activeQueues.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
