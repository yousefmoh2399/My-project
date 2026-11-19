using System;

namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Immutable record representing a player's pending application to join an alliance.
    /// </summary>
    public sealed class AllianceApplication : IEquatable<AllianceApplication>
    {
        public string ApplicationId { get; }
        public AllianceId AllianceId { get; }
        public string PlayerId { get; }
        public string PlayerName { get; }
        public long CombatPower { get; }
        public DateTime AppliedAtUtc { get; }

        public AllianceApplication(
            string applicationId,
            AllianceId allianceId,
            string playerId,
            string playerName,
            long combatPower,
            DateTime appliedAtUtc)
        {
            ApplicationId = applicationId ?? string.Empty;
            AllianceId = allianceId;
            PlayerId = playerId ?? string.Empty;
            PlayerName = playerName ?? "Commander";
            CombatPower = Math.Max(0, combatPower);
            AppliedAtUtc = appliedAtUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(appliedAtUtc, DateTimeKind.Utc)
                : appliedAtUtc.ToUniversalTime();
        }

        public bool Equals(AllianceApplication other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ApplicationId, other.ApplicationId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || (obj is AllianceApplication other && Equals(other));

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(ApplicationId ?? string.Empty);
    }
}
