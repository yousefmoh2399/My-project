using System;

namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Type-safe immutable identifier for player base building instances (e.g. "bld_hq", "bld_barracks_01").
    /// Never use GameObject names or scene hierarchy paths as persistent IDs.
    /// </summary>
    public readonly struct BuildingId : IEquatable<BuildingId>, IComparable<BuildingId>
    {
        public static readonly BuildingId Empty = new BuildingId(string.Empty);

        private readonly string _value;
        public string Value => _value ?? string.Empty;

        public bool IsValid => !string.IsNullOrWhiteSpace(Value) && Value.StartsWith("bld_");

        public BuildingId(string value)
        {
            _value = value?.Trim() ?? string.Empty;
        }

        public bool Equals(BuildingId other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is BuildingId other && Equals(other);

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public int CompareTo(BuildingId other) => string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override string ToString() => Value;

        public static bool operator ==(BuildingId left, BuildingId right) => left.Equals(right);

        public static bool operator !=(BuildingId left, BuildingId right) => !left.Equals(right);

        public static implicit operator string(BuildingId id) => id.Value;
    }
}
