using System;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Immutable hourly production yield rates for all core resources.
    /// </summary>
    public readonly struct ResourceYieldRate : IEquatable<ResourceYieldRate>
    {
        public long GoldPerHour { get; }
        public long OilPerHour { get; }
        public long IronPerHour { get; }

        public ResourceYieldRate(long goldPerHour, long oilPerHour, long ironPerHour)
        {
            GoldPerHour = Math.Max(0, goldPerHour);
            OilPerHour = Math.Max(0, oilPerHour);
            IronPerHour = Math.Max(0, ironPerHour);
        }

        public long GetRate(ResourceType type)
        {
            return type switch
            {
                ResourceType.Gold => GoldPerHour,
                ResourceType.Oil => OilPerHour,
                ResourceType.Iron => IronPerHour,
                _ => 0
            };
        }

        public bool Equals(ResourceYieldRate other) =>
            GoldPerHour == other.GoldPerHour && OilPerHour == other.OilPerHour && IronPerHour == other.IronPerHour;

        public override bool Equals(object obj) => obj is ResourceYieldRate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(GoldPerHour, OilPerHour, IronPerHour);

        public static bool operator ==(ResourceYieldRate left, ResourceYieldRate right) => left.Equals(right);
        public static bool operator !=(ResourceYieldRate left, ResourceYieldRate right) => !left.Equals(right);
    }
}
