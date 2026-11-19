using System;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Economy;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;

namespace KingWorldWar.Infrastructure.Economy
{
    /// <summary>
    /// Client-side service managing economy snapshots, continuous projection estimation,
    /// and routing spend requests to the authoritative repository.
    /// Implements both IEconomyService and IResourceState for seamless backward compatibility.
    /// Possesses zero write paths for arbitrary balance alteration.
    /// </summary>
    public sealed class EconomyService : IEconomyService, IResourceState
    {
        private readonly IEconomyRepository _repository;
        private readonly ILog _log;
        private readonly object _lock = new();

        private EconomySnapshot _currentSnapshot;
        private EconomySnapshot _currentProjection;
        private TimeSpan _serverTimeOffset = TimeSpan.Zero;
        private bool _isLoading;
        private bool _isStale;

        public EconomySnapshot CurrentSnapshot => _currentSnapshot;
        public EconomySnapshot CurrentProjection => _currentProjection;
        public TimeSpan ServerTimeOffset => _serverTimeOffset;
        public bool IsLoading => _isLoading;
        public bool IsStale => _isStale;
        public bool IsMock => _repository.IsMock;

        public DateTime EstimatedCurrentServerTimeUtc => DateTime.UtcNow.Add(_serverTimeOffset);

        // IResourceState implementation
        public long Gold => _currentProjection?.Gold ?? _currentSnapshot?.Gold ?? 0;
        public long Oil => _currentProjection?.Oil ?? _currentSnapshot?.Oil ?? 0;
        public long Iron => _currentProjection?.Iron ?? _currentSnapshot?.Iron ?? 0;

        public event Action<EconomySnapshot> OnSnapshotUpdated;
        public event Action<EconomySnapshot> OnProjectionTick;
        public event Action OnResourcesChanged;

        public EconomyService(IEconomyRepository repository, ILog log = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _log = log ?? GameLog.Default;

            // Default safe starting snapshot while loading
            _currentSnapshot = new EconomySnapshot(
                0, 0, 0,
                new StorageCapacity(50000, 50000, 50000),
                new ResourceYieldRate(0, 0, 0),
                DateTime.UtcNow,
                0);
            _currentProjection = _currentSnapshot;
        }

        public async Task<Result<EconomySnapshot, GameError>> RefreshSnapshotAsync(CancellationToken ct = default)
        {
            _isLoading = true;
            try
            {
                var result = await _repository.GetSnapshotAsync(ct).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    ApplyAuthoritativeSnapshot(result.Value);
                    _isStale = false;
                    return Result<EconomySnapshot, GameError>.Ok(_currentSnapshot);
                }

                _isStale = true;
                _log.Warning("Economy", $"Failed to fetch snapshot: {result.Error}");
                return Result<EconomySnapshot, GameError>.Fail(result.Error);
            }
            catch (Exception ex)
            {
                _isStale = true;
                _log.Error("Economy", $"Exception refreshing snapshot: {ex.Message}", ex);
                return Result<EconomySnapshot, GameError>.Fail(GameError.NetworkUnavailable(ex.Message));
            }
            finally
            {
                _isLoading = false;
            }
        }

        public async Task<Result<TransactionResult, GameError>> RequestSpendAsync(
            string requestId,
            long gold,
            long oil,
            long iron,
            string reason,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return Result<TransactionResult, GameError>.Fail(GameError.InvalidPayload("requestId is required."));
            }

            long expectedVersion = _currentSnapshot?.SnapshotVersion ?? 0;
            var result = await _repository.RequestSpendAsync(
                requestId, gold, oil, iron, reason, expectedVersion, ct).ConfigureAwait(false);

            if (result.IsSuccess && result.Value.Success)
            {
                if (result.Value.AuthoritativeSnapshot != null)
                {
                    ApplyAuthoritativeSnapshot(result.Value.AuthoritativeSnapshot);
                }
                return Result<TransactionResult, GameError>.Ok(result.Value);
            }

            _log.Warning("Economy", $"Spend request '{requestId}' failed: {result.Error}");
            return result;
        }

        public bool CanAfford(long gold, long oil, long iron)
        {
            var projection = _currentProjection ?? _currentSnapshot;
            return projection != null && projection.CanAfford(gold, oil, iron);
        }

        public (long GoldShortfall, long OilShortfall, long IronShortfall) GetShortfall(
            long gold, long oil, long iron)
        {
            var projection = _currentProjection ?? _currentSnapshot;
            if (projection == null) return (gold, oil, iron);
            return projection.GetShortfall(gold, oil, iron);
        }

        public void TickProjection()
        {
            lock (_lock)
            {
                if (_currentSnapshot == null) return;
                var updated = _currentSnapshot.CalculateProjection(EstimatedCurrentServerTimeUtc);
                _currentProjection = updated;
            }

            OnProjectionTick?.Invoke(_currentProjection);
            OnResourcesChanged?.Invoke();
        }

        public void ApplyAuthoritativeSnapshot(EconomySnapshot snapshot)
        {
            if (snapshot == null) return;

            lock (_lock)
            {
                _currentSnapshot = snapshot;
                _serverTimeOffset = snapshot.LastUpdatedServerTimeUtc - DateTime.UtcNow;
                _currentProjection = snapshot.CalculateProjection(EstimatedCurrentServerTimeUtc);
            }

            _log.Info("Economy", $"Snapshot v{snapshot.SnapshotVersion} applied. Offset: {_serverTimeOffset.TotalSeconds:F2}s");
            OnSnapshotUpdated?.Invoke(_currentSnapshot);
            OnResourcesChanged?.Invoke();
        }
    }
}
