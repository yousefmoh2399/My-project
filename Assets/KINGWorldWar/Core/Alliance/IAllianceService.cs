using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Domain service managing active player alliance state, member directory,
    /// pending applications, and events for reactive UI updates.
    /// </summary>
    public interface IAllianceService
    {
        AllianceProfile CurrentAlliance { get; }
        AllianceRole CurrentUserRole { get; }
        bool HasAlliance { get; }
        IReadOnlyList<AllianceMember> CurrentMembers { get; }
        IReadOnlyList<AllianceApplication> PendingApplications { get; }

        event Action<AllianceProfile> OnAllianceUpdated;
        event Action<IReadOnlyList<AllianceMember>> OnMembersUpdated;
        event Action<IReadOnlyList<AllianceApplication>> OnApplicationsUpdated;
        event Action OnLeftAlliance;

        Task<Result<bool, GameError>> RefreshCurrentAllianceAsync(CancellationToken ct = default);
        Task<Result<IReadOnlyList<AllianceProfile>, GameError>> SearchAlliancesAsync(string query, AllianceJoinPolicy? policyFilter = null, CancellationToken ct = default);
        Task<Result<AllianceProfile, GameError>> CreateAllianceAsync(string name, string tag, string notice, AllianceJoinPolicy policy, CancellationToken ct = default);
        Task<Result<bool, GameError>> JoinAllianceAsync(AllianceId allianceId, CancellationToken ct = default);
        Task<Result<bool, GameError>> ApplyToAllianceAsync(AllianceId allianceId, CancellationToken ct = default);
        Task<Result<bool, GameError>> AcceptApplicationAsync(string applicationId, CancellationToken ct = default);
        Task<Result<bool, GameError>> RejectApplicationAsync(string applicationId, CancellationToken ct = default);
        Task<Result<bool, GameError>> KickMemberAsync(string targetPlayerId, CancellationToken ct = default);
        Task<Result<bool, GameError>> SetMemberRoleAsync(string targetPlayerId, AllianceRole newRole, CancellationToken ct = default);
        Task<Result<bool, GameError>> LeaveAllianceAsync(CancellationToken ct = default);
        Task<Result<bool, GameError>> UpdateNoticeAsync(string newNotice, CancellationToken ct = default);
    }
}
