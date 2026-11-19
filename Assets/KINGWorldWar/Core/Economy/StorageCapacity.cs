using System;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Immutable storage capacity limits for all core resources.
    /// </summary>
    public readonly struct StorageCapacity : IEquatable<StorageCapacity>
    {
        public long GoldCap { get; }
        public long OilCap { get; }
        public long IronCap { get; }

        public StorageCapacity(long goldCap, long oilCap, long ironCap)
        {
            GoldCap = Math.Max(0, goldCap);
            OilCap = Math.Max(0, oilCap);
            IronCap = Math.Max(0, ironCap);
        }

        public long GetCap(ResourceType type)
        {
            return type switch
            {
                ResourceType.Gold => GoldCap,
                ResourceType.Oil => OilCap,
                ResourceType.Iron => IronCap,
                _ => 0
            };
        }

        public bool Equals(StorageCapacity other) =>
            GoldCap == other.GoldCap && OilCap == other.OilCap && IronCap == other.IronCap;

        public override bool Equals(object obj) => obj is StorageCapacity other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(GoldCap, OilCap, IronCap);

        public static bool operator ==(StorageCapacity left, StorageCapacity right) => left.Equals(right);
        public static bool operator !=(StorageCapacity left, StorageCapacity right) => !left.Equals(right);
    }
}
