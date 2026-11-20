using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# domain contract orchestrating military recruitment, prerequisites,
    /// standing army roster management, and training queue state transitions.
    /// Follows Untrusted Client principles: all mutations route through IArmyRepository.
    /// </summary>
    public interface IArmyService
    {
        IArmyDataSource DataSource { get; }
        IArmyRepository Repository { get; }
        ArmyRosterSnapshot CurrentSnapshot { get; }
        IReadOnlyList<RecruitmentQueueItem> ActiveQueues { get; }
        int MaxConcurrentQueues { get; }

        ArmyRoster GetRoster();
        int GetTotalCombatPower();
        int GetTotalForcesCount();
        IReadOnlyList<RecruitmentQueueItem> GetActiveQueues();

        bool CheckPrerequisites(UnitType type, out string reason);

        Task<Result<ArmyRosterSnapshot, GameError>> RefreshRosterAsync(CancellationToken ct = default);
        Task<Result<RecruitmentQueueItem, GameError>> RequestRecruitmentAsync(UnitType type, int count, CancellationToken ct = default);
        Task<Result<bool, GameError>> CancelRecruitmentAsync(string queueId, CancellationToken ct = default);
        Task<Result<bool, GameError>> RequestUnitUpgradeAsync(UnitType type, CancellationToken ct = default);
        Result<int, GameError> CheckCompletedRecruitments(DateTime utcNow);

        event Action<RecruitmentQueueItem> OnRecruitmentStarted;
        event Action<string> OnRecruitmentCancelled;
        event Action<UnitType, int> OnRecruitmentCompleted;
        event Action OnRosterUpdated;
        event Action<IReadOnlyList<RecruitmentQueueItem>> OnQueueUpdated;
    }
}
