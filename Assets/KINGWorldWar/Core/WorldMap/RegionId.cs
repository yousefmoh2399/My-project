using System;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Type-safe immutable identifier for world map regions (e.g. "reg_north").
    /// </summary>
    public readonly struct RegionId : IEquatable<RegionId>, IComparable<RegionId>
    {
        public static readonly RegionId Empty = new RegionId(string.Empty);

        public string Value { get; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Value) && Value.StartsWith("reg_");

        public RegionId(string value)
        {
            Value = value?.Trim() ?? string.Empty;
        }

        public bool Equals(RegionId other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is RegionId other && Equals(other);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);

        public int CompareTo(RegionId other) => string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(RegionId left, RegionId right) => left.Equals(right);

        public static bool operator !=(RegionId left, RegionId right) => !left.Equals(right);

        public static implicit operator string(RegionId id) => id.Value;
    }
}
