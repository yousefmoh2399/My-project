using System;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Service managing client-side economic projections, server clock synchronization,
    /// and routing mutation requests to the authoritative repository.
    /// Has ZERO write paths for client code to set arbitrary balances.
    /// </summary>
    public interface IEconomyService
    {
        EconomySnapshot CurrentSnapshot { get; }
        EconomySnapshot CurrentProjection { get; }
        DateTime EstimatedCurrentServerTimeUtc { get; }
        TimeSpan ServerTimeOffset { get; }
        bool IsLoading { get; }
        bool IsStale { get; }
        bool IsMock { get; }

        event Action<EconomySnapshot> OnSnapshotUpdated;
        event Action<EconomySnapshot> OnProjectionTick;

        Task<Result<EconomySnapshot, GameError>> RefreshSnapshotAsync(CancellationToken ct = default);

        Task<Result<TransactionResult, GameError>> RequestSpendAsync(
            string requestId,
            long gold,
            long oil,
            long iron,
            string reason,
            CancellationToken ct = default);

        bool CanAfford(long gold, long oil, long iron);

        (long GoldShortfall, long OilShortfall, long IronShortfall) GetShortfall(
            long gold, long oil, long iron);

        void TickProjection();
    }
}
