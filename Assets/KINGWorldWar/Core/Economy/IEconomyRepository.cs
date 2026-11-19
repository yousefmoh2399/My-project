using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Contract for server-authoritative economy repository operations.
    /// In production builds, this communicates exclusively with Nakama RPCs.
    /// In development builds, this resolves to LocalMockEconomyRepository.
    /// </summary>
    public interface IEconomyRepository
    {
        bool IsMock { get; }

        Task<Result<EconomySnapshot, GameError>> GetSnapshotAsync(CancellationToken ct = default);

        Task<Result<TransactionResult, GameError>> RequestSpendAsync(
            string requestId,
            long gold,
            long oil,
            long iron,
            string reason,
            long expectedVersion,
            CancellationToken ct = default);
    }
}
