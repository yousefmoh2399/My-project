using System;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Immutable server-authoritative state snapshot of a player's economic balances,
    /// production rates, storage caps, and optimistic concurrency version.
    /// </summary>
    public sealed class EconomySnapshot : IEquatable<EconomySnapshot>
    {
        public long Gold { get; }
        public long Oil { get; }
        public long Iron { get; }
        public StorageCapacity StorageCaps { get; }
        public ResourceYieldRate ProductionRatesPerHour { get; }
        public DateTime LastUpdatedServerTimeUtc { get; }
        public long SnapshotVersion { get; }

        public EconomySnapshot(
            long gold,
            long oil,
            long iron,
            StorageCapacity storageCaps,
            ResourceYieldRate productionRatesPerHour,
            DateTime lastUpdatedServerTimeUtc,
            long snapshotVersion)
        {
            Gold = Math.Max(0, gold);
            Oil = Math.Max(0, oil);
            Iron = Math.Max(0, iron);
            StorageCaps = storageCaps;
            ProductionRatesPerHour = productionRatesPerHour;
            LastUpdatedServerTimeUtc = lastUpdatedServerTimeUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(lastUpdatedServerTimeUtc, DateTimeKind.Utc)
                : lastUpdatedServerTimeUtc.ToUniversalTime();
            SnapshotVersion = Math.Max(0, snapshotVersion);
        }

        public long GetBalance(ResourceType type)
        {
            return type switch
            {
                ResourceType.Gold => Gold,
                ResourceType.Oil => Oil,
                ResourceType.Iron => Iron,
                _ => 0
            };
        }

        public bool CanAfford(long goldCost, long oilCost, long ironCost)
        {
            return Gold >= Math.Max(0, goldCost) &&
                   Oil >= Math.Max(0, oilCost) &&
                   Iron >= Math.Max(0, ironCost);
        }

        public (long GoldShortfall, long OilShortfall, long IronShortfall) GetShortfall(
            long goldCost, long oilCost, long ironCost)
        {
            long gShort = Math.Max(0, goldCost - Gold);
            long oShort = Math.Max(0, oilCost - Oil);
            long iShort = Math.Max(0, ironCost - Iron);
            return (gShort, oShort, iShort);
        }

        /// <summary>
        /// Pure projection formula computing continuous accrual from server timestamp offset.
        /// Client estimate only — server remains the sole authoritative arbiter of mutations.
        /// </summary>
        public EconomySnapshot CalculateProjection(DateTime estimatedCurrentServerTimeUtc)
        {
            var targetUtc = estimatedCurrentServerTimeUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(estimatedCurrentServerTimeUtc, DateTimeKind.Utc)
                : estimatedCurrentServerTimeUtc.ToUniversalTime();

            if (targetUtc <= LastUpdatedServerTimeUtc)
            {
                return this;
            }

            double elapsedSeconds = (targetUtc - LastUpdatedServerTimeUtc).TotalSeconds;
            if (elapsedSeconds <= 0) return this;

            long accruedGold = CalculateAccrual(elapsedSeconds, ProductionRatesPerHour.GoldPerHour);
            long accruedOil = CalculateAccrual(elapsedSeconds, ProductionRatesPerHour.OilPerHour);
            long accruedIron = CalculateAccrual(elapsedSeconds, ProductionRatesPerHour.IronPerHour);

            long projectedGold = Math.Min(StorageCaps.GoldCap, ClampAdd(Gold, accruedGold));
            long projectedOil = Math.Min(StorageCaps.OilCap, ClampAdd(Oil, accruedOil));
            long projectedIron = Math.Min(StorageCaps.IronCap, ClampAdd(Iron, accruedIron));

            return new EconomySnapshot(
                projectedGold,
                projectedOil,
                projectedIron,
                StorageCaps,
                ProductionRatesPerHour,
                LastUpdatedServerTimeUtc, // Original anchor timestamp preserved
                SnapshotVersion);
        }

        private static long CalculateAccrual(double elapsedSeconds, long ratePerHour)
        {
            if (ratePerHour <= 0 || elapsedSeconds <= 0) return 0;
            // Floor of (elapsedSeconds * ratePerHour / 3600.0)
            double accrual = Math.Floor((elapsedSeconds * ratePerHour) / 3600.0);
            if (accrual > long.MaxValue) return long.MaxValue;
            return (long)accrual;
        }

        private static long ClampAdd(long baseVal, long addVal)
        {
            try
            {
                checked { return baseVal + addVal; }
            }
            catch (OverflowException)
            {
                return long.MaxValue;
            }
        }

        /// <summary>
        /// Validates payload integrity, preventing negative balances, inverted timestamps, or corrupted revisions.
        /// </summary>
        public static Result<EconomySnapshot, GameError> Validate(
            long gold,
            long oil,
            long iron,
            StorageCapacity storageCaps,
            ResourceYieldRate productionRates,
            DateTime serverTimeUtc,
            long version)
        {
            if (gold < 0) return Result<EconomySnapshot, GameError>.Fail(GameError.InvalidPayload("Gold balance cannot be negative."));
            if (oil < 0) return Result<EconomySnapshot, GameError>.Fail(GameError.InvalidPayload("Oil balance cannot be negative."));
            if (iron < 0) return Result<EconomySnapshot, GameError>.Fail(GameError.InvalidPayload("Iron balance cannot be negative."));
            if (version < 0) return Result<EconomySnapshot, GameError>.Fail(GameError.InvalidPayload("SnapshotVersion cannot be negative."));
            if (serverTimeUtc == default) return Result<EconomySnapshot, GameError>.Fail(GameError.InvalidPayload("Server timestamp cannot be default."));

            return Result<EconomySnapshot, GameError>.Ok(new EconomySnapshot(
                gold, oil, iron, storageCaps, productionRates, serverTimeUtc, version));
        }

        public bool Equals(EconomySnapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Gold == other.Gold &&
                   Oil == other.Oil &&
                   Iron == other.Iron &&
                   StorageCaps.Equals(other.StorageCaps) &&
                   ProductionRatesPerHour.Equals(other.ProductionRatesPerHour) &&
                   LastUpdatedServerTimeUtc.Equals(other.LastUpdatedServerTimeUtc) &&
                   SnapshotVersion == other.SnapshotVersion;
        }

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is EconomySnapshot other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Gold, Oil, Iron, StorageCaps, ProductionRatesPerHour, LastUpdatedServerTimeUtc, SnapshotVersion);
    }
}
