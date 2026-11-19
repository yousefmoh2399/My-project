using System;

namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Resource requirements (Gold, Oil, Iron) to build or upgrade a base structure.
    /// Non-authoritative display model on client; validated server-side by Nakama.
    /// </summary>
    [Serializable]
    public readonly struct BuildingCost : IEquatable<BuildingCost>
    {
        public static readonly BuildingCost Zero = new BuildingCost(0, 0, 0);

        public long Gold { get; }
        public long Oil { get; }
        public long Iron { get; }

        public BuildingCost(long gold, long oil, long iron)
        {
            Gold = Math.Max(0, gold);
            Oil = Math.Max(0, oil);
            Iron = Math.Max(0, iron);
        }

        public bool CanAfford(long playerGold, long playerOil, long playerIron)
        {
            return playerGold >= Gold && playerOil >= Oil && playerIron >= Iron;
        }

        public bool Equals(BuildingCost other)
        {
            return Gold == other.Gold && Oil == other.Oil && Iron == other.Iron;
        }

        public override bool Equals(object obj) => obj is BuildingCost other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Gold, Oil, Iron);

        public static bool operator ==(BuildingCost left, BuildingCost right) => left.Equals(right);

        public static bool operator !=(BuildingCost left, BuildingCost right) => !left.Equals(right);

        public override string ToString() => $"[Cost G:{Gold} O:{Oil} I:{Iron}]";
    }
}
