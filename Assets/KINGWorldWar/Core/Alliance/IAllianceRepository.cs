using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Server-authoritative contract for alliance data persistence and RPC operations.
    /// In production, calls Nakama RPCs; in Editor/tests, backed by LocalMockAllianceRepository.
    /// </summary>
    public interface IAllianceRepository
    {
        bool IsMock { get; }

        Task<Result<AllianceProfile, GameError>> GetAllianceAsync(AllianceId id, CancellationToken ct = default);

        Task<Result<IReadOnlyList<AllianceProfile>, GameError>> SearchAlliancesAsync(string query, AllianceJoinPolicy? policyFilter = null, CancellationToken ct = default);

        Task<Result<IReadOnlyList<AllianceMember>, GameError>> GetMembersAsync(AllianceId id, CancellationToken ct = default);

        Task<Result<IReadOnlyList<AllianceApplication>, GameError>> GetApplicationsAsync(AllianceId id, CancellationToken ct = default);

        Task<Result<AllianceProfile, GameError>> CreateAllianceAsync(string requestId, string name, string tag, string notice, AllianceJoinPolicy policy, CancellationToken ct = default);

        Task<Result<bool, GameError>> JoinAllianceAsync(string requestId, AllianceId id, CancellationToken ct = default);

        Task<Result<bool, GameError>> ApplyToAllianceAsync(string requestId, AllianceId id, CancellationToken ct = default);

        Task<Result<bool, GameError>> AcceptApplicationAsync(string requestId, AllianceId id, string applicationId, CancellationToken ct = default);

        Task<Result<bool, GameError>> RejectApplicationAsync(string requestId, AllianceId id, string applicationId, CancellationToken ct = default);

        Task<Result<bool, GameError>> KickMemberAsync(string requestId, AllianceId id, string targetPlayerId, CancellationToken ct = default);

        Task<Result<bool, GameError>> SetMemberRoleAsync(string requestId, AllianceId id, string targetPlayerId, AllianceRole newRole, CancellationToken ct = default);

        Task<Result<bool, GameError>> LeaveAllianceAsync(string requestId, AllianceId id, CancellationToken ct = default);

        Task<Result<bool, GameError>> UpdateNoticeAsync(string requestId, AllianceId id, string newNotice, CancellationToken ct = default);
    }
}
