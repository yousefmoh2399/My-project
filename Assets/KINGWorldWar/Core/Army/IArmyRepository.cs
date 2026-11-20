using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Server-authoritative repository contract for military army roster, training queues, and unit upgrades.
    /// In production, calls authenticated Nakama RPCs; in dev/testing, backed by LocalMockArmyRepository.
    /// All mutations require client-generated UUIDv4 requestId for idempotency.
    /// </summary>
    public interface IArmyRepository
    {
        bool IsMock { get; }

        Task<Result<ArmyRosterSnapshot, GameError>> GetRosterAsync(CancellationToken ct = default);

        Task<Result<IReadOnlyList<RecruitmentQueueItem>, GameError>> GetTrainingQueueAsync(CancellationToken ct = default);

        Task<Result<RecruitmentQueueItem, GameError>> RequestTrainingAsync(
            string requestId,
            UnitType unitType,
            int quantity,
            CancellationToken ct = default);

        Task<Result<bool, GameError>> RequestCancelTrainingAsync(
            string requestId,
            string queueItemId,
            CancellationToken ct = default);

        Task<Result<bool, GameError>> RequestUnitUpgradeAsync(
            string requestId,
            UnitType unitType,
            CancellationToken ct = default);
    }
}
