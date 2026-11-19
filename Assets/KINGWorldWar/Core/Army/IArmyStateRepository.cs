using System.Collections.Generic;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# interface for persisting and querying player army roster and recruitment queues.
    /// In Phase 6, backed by InMemoryArmyStateRepository.
    /// In Phase 8, backed by Nakama storage / server state.
    /// </summary>
    public interface IArmyStateRepository
    {
        ArmyRoster GetRoster();
        void UpdateRoster(ArmyRoster roster);

        IReadOnlyList<RecruitmentQueueItem> GetActiveQueues();
        bool TryGetQueueItem(string queueId, out RecruitmentQueueItem item);
        void EnqueueRecruitment(RecruitmentQueueItem item);
        bool RemoveQueueItem(string queueId);
        int MaxConcurrentQueues { get; }
    }
}
