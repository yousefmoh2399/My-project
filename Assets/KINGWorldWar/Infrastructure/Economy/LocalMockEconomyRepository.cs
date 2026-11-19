#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Economy;

namespace KingWorldWar.Infrastructure.Economy
{
    /// <summary>
    /// Editor/Development-only mock implementation of IEconomyRepository.
    /// Simulates server latency, continuous passive accrual, optimistic concurrency version checks,
    /// idempotency key caching, and insufficient resource validation.
    /// Strictly excluded from release builds.
    /// </summary>
    public sealed class LocalMockEconomyRepository : IEconomyRepository
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, TransactionResult> _processedRequests = new();

        private long _gold;
        private long _oil;
        private long _iron;
        private StorageCapacity _storageCaps;
        private ResourceYieldRate _productionRates;
        private DateTime _lastUpdatedServerTimeUtc;
        private long _snapshotVersion;

        public bool IsMock => true;
        public int SimulatedLatencyMs { get; set; } = 20;

        public LocalMockEconomyRepository(
            long initialGold = 125000,
            long initialOil = 48000,
            long initialIron = 89000,
            StorageCapacity? storageCaps = null,
            ResourceYieldRate? productionRates = null)
        {
            _gold = Math.Max(0, initialGold);
            _oil = Math.Max(0, initialOil);
            _iron = Math.Max(0, initialIron);
            _storageCaps = storageCaps ?? new StorageCapacity(200000, 200000, 200000);
            _productionRates = productionRates ?? new ResourceYieldRate(4000, 2800, 2000);
            _lastUpdatedServerTimeUtc = DateTime.UtcNow;
            _snapshotVersion = 1;
        }

        public async Task<Result<EconomySnapshot, GameError>> GetSnapshotAsync(CancellationToken ct = default)
        {
            if (SimulatedLatencyMs > 0)
            {
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);
            }

            lock (_lock)
            {
                ReconcileAccrual_Internal();
                var snapshot = CreateCurrentSnapshot_Internal();
                return Result<EconomySnapshot, GameError>.Ok(snapshot);
            }
        }

        public async Task<Result<TransactionResult, GameError>> RequestSpendAsync(
            string requestId,
            long gold,
            long oil,
            long iron,
            string reason,
            long expectedVersion,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return Result<TransactionResult, GameError>.Fail(GameError.InvalidPayload("requestId cannot be null or empty."));
            }

            if (SimulatedLatencyMs > 0)
            {
                await Task.Delay(SimulatedLatencyMs, ct).ConfigureAwait(false);
            }

            lock (_lock)
            {
                // 1. Idempotency Check: if requestId was already processed, return original cached response
                if (_processedRequests.TryGetValue(requestId, out var cachedResult))
                {
                    return Result<TransactionResult, GameError>.Ok(cachedResult);
                }

                // 2. Accrual Reconciliation before validating balances
                ReconcileAccrual_Internal();

                // 3. Optimistic Concurrency Check
                if (expectedVersion > 0 && expectedVersion != _snapshotVersion)
                {
                    var staleErr = GameError.StaleVersion(expectedVersion, _snapshotVersion);
                    var failRes = TransactionResult.Fail(staleErr, requestId, CreateCurrentSnapshot_Internal());
                    return Result<TransactionResult, GameError>.Fail(staleErr);
                }

                // 4. Validate Sufficient Balances
                long gCost = Math.Max(0, gold);
                long oCost = Math.Max(0, oil);
                long iCost = Math.Max(0, iron);

                if (_gold < gCost || _oil < oCost || _iron < iCost)
                {
                    long gShort = Math.Max(0, gCost - _gold);
                    long oShort = Math.Max(0, oCost - _oil);
                    long iShort = Math.Max(0, iCost - _iron);
                    string details = $"Shortfall: G:{gShort}, O:{oShort}, I:{iShort}";
                    var insErr = GameError.InsufficientResources(details);
                    return Result<TransactionResult, GameError>.Fail(insErr);
                }

                // 5. Execute Atomic Debit
                _gold -= gCost;
                _oil -= oCost;
                _iron -= iCost;
                _snapshotVersion++;
                _lastUpdatedServerTimeUtc = DateTime.UtcNow;

                var newSnapshot = CreateCurrentSnapshot_Internal();
                string txId = $"mock_tx_{Guid.NewGuid():N}";
                var txResult = TransactionResult.Ok(newSnapshot, txId, requestId, _lastUpdatedServerTimeUtc);

                // Cache for idempotency
                _processedRequests[requestId] = txResult;

                return Result<TransactionResult, GameError>.Ok(txResult);
            }
        }

        private void ReconcileAccrual_Internal()
        {
            var now = DateTime.UtcNow;
            if (now <= _lastUpdatedServerTimeUtc) return;

            double elapsedSec = (now - _lastUpdatedServerTimeUtc).TotalSeconds;
            if (elapsedSec <= 0) return;

            long addG = (long)Math.Floor((elapsedSec * _productionRates.GoldPerHour) / 3600.0);
            long addO = (long)Math.Floor((elapsedSec * _productionRates.OilPerHour) / 3600.0);
            long addI = (long)Math.Floor((elapsedSec * _productionRates.IronPerHour) / 3600.0);

            _gold = Math.Min(_storageCaps.GoldCap, _gold + addG);
            _oil = Math.Min(_storageCaps.OilCap, _oil + addO);
            _iron = Math.Min(_storageCaps.IronCap, _iron + addI);

            _lastUpdatedServerTimeUtc = now;
        }

        private EconomySnapshot CreateCurrentSnapshot_Internal()
        {
            return new EconomySnapshot(
                _gold,
                _oil,
                _iron,
                _storageCaps,
                _productionRates,
                _lastUpdatedServerTimeUtc,
                _snapshotVersion);
        }
    }
}
#endif
