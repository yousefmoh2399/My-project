using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Pure C# domain contract orchestrating military recruitment, prerequisites,
    /// and army roster management.
    /// Follows Untrusted Client principles: exposes asynchronous Result-based contracts
    /// designed for drop-in substitution by server-authoritative Nakama RPCs in Phase 8.
    /// </summary>
    public interface IArmyService
    {
        IArmyDataSource DataSource { get; }
        IArmyStateRepository StateRepository { get; }

        ArmyRoster GetRoster();
        int GetTotalCombatPower();
        IReadOnlyList<RecruitmentQueueItem> GetActiveQueues();

        bool CheckPrerequisites(UnitType type, out string reason);

        Task<Result<RecruitmentQueueItem, GameError>> RequestRecruitmentAsync(UnitType type, int count, CancellationToken ct = default);
        Task<Result<GameError>> CancelRecruitmentAsync(string queueId, CancellationToken ct = default);
        Result<int, GameError> CheckCompletedRecruitments(DateTime utcNow);

        event Action<RecruitmentQueueItem> OnRecruitmentStarted;
        event Action<string> OnRecruitmentCancelled;
        event Action<UnitType, int> OnRecruitmentCompleted;
        event Action OnRosterUpdated;
    }
}
