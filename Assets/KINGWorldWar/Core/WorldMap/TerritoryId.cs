using System;

namespace KingWorldWar.Core.WorldMap
{
    /// <summary>
    /// Type-safe immutable identifier for world map territories (e.g. "ter_nrd_001").
    /// Never use GameObject names or scene paths as persistent IDs.
    /// </summary>
    public readonly struct TerritoryId : IEquatable<TerritoryId>, IComparable<TerritoryId>
    {
        public static readonly TerritoryId Empty = new TerritoryId(string.Empty);

        public string Value { get; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Value) && Value.StartsWith("ter_");

        public TerritoryId(string value)
        {
            Value = value?.Trim() ?? string.Empty;
        }

        public bool Equals(TerritoryId other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is TerritoryId other && Equals(other);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);

        public int CompareTo(TerritoryId other) => string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(TerritoryId left, TerritoryId right) => left.Equals(right);

        public static bool operator !=(TerritoryId left, TerritoryId right) => !left.Equals(right);

        public static implicit operator string(TerritoryId id) => id.Value;
    }
}
