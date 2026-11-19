using System;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Type-safe immutable identifier for military units.
    /// Handles default values gracefully so default(UnitId) equals UnitId.Empty.
    /// </summary>
    public readonly struct UnitId : IEquatable<UnitId>, IComparable<UnitId>
    {
        private readonly string _value;

        public string Value => _value ?? string.Empty;

        public static UnitId Empty => new UnitId(string.Empty);

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        public UnitId(string value)
        {
            _value = value?.Trim() ?? string.Empty;
        }

        public bool Equals(UnitId other) =>
            string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            obj is UnitId other && Equals(other);

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public int CompareTo(UnitId other) =>
            string.Compare(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override string ToString() => Value;

        public static bool operator ==(UnitId left, UnitId right) => left.Equals(right);
        public static bool operator !=(UnitId left, UnitId right) => !left.Equals(right);
    }
}
