using System;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Resource expenditures and training duration required to recruit a unit or batch of units.
    /// </summary>
    [Serializable]
    public readonly struct UnitCost : IEquatable<UnitCost>
    {
        public int Gold { get; }
        public int Oil { get; }
        public int Iron { get; }
        public int RecruitDurationSeconds { get; }

        public UnitCost(int gold, int oil, int iron, int recruitDurationSeconds)
        {
            Gold = Math.Max(0, gold);
            Oil = Math.Max(0, oil);
            Iron = Math.Max(0, iron);
            RecruitDurationSeconds = Math.Max(0, recruitDurationSeconds);
        }

        public UnitCost Multiply(int count)
        {
            if (count <= 0) return new UnitCost(0, 0, 0, 0);
            return new UnitCost(Gold * count, Oil * count, Iron * count, RecruitDurationSeconds * count);
        }

        public bool Equals(UnitCost other) =>
            Gold == other.Gold &&
            Oil == other.Oil &&
            Iron == other.Iron &&
            RecruitDurationSeconds == other.RecruitDurationSeconds;

        public override bool Equals(object obj) =>
            obj is UnitCost other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Gold, Oil, Iron, RecruitDurationSeconds);

        public static bool operator ==(UnitCost left, UnitCost right) => left.Equals(right);
        public static bool operator !=(UnitCost left, UnitCost right) => !left.Equals(right);

        public override string ToString() =>
            $"Gold:{Gold} Oil:{Oil} Iron:{Iron} Time:{RecruitDurationSeconds}s";
    }
}
