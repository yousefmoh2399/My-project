using System;

namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Strongly typed, immutable identifier for an Alliance/Coalition.
    /// </summary>
    public readonly struct AllianceId : IEquatable<AllianceId>
    {
        public static readonly AllianceId Empty = new AllianceId(string.Empty);

        public string Value { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(Value);

        public AllianceId(string value)
        {
            Value = value?.Trim() ?? string.Empty;
        }

        public bool Equals(AllianceId other) =>
            string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            obj is AllianceId other && Equals(other);

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);

        public override string ToString() => Value;

        public static bool operator ==(AllianceId left, AllianceId right) => left.Equals(right);
        public static bool operator !=(AllianceId left, AllianceId right) => !left.Equals(right);
    }
}
