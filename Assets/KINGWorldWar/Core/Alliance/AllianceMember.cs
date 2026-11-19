using System;

namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Immutable domain model representing a player member within an alliance.
    /// </summary>
    public sealed class AllianceMember : IEquatable<AllianceMember>
    {
        public string PlayerId { get; }
        public string PlayerName { get; }
        public AllianceRole Role { get; }
        public long CombatPower { get; }
        public long ContributionScore { get; }
        public DateTime JoinedAtUtc { get; }

        public AllianceMember(
            string playerId,
            string playerName,
            AllianceRole role,
            long combatPower,
            long contributionScore,
            DateTime joinedAtUtc)
        {
            PlayerId = playerId ?? string.Empty;
            PlayerName = playerName ?? "Commander";
            Role = role;
            CombatPower = Math.Max(0, combatPower);
            ContributionScore = Math.Max(0, contributionScore);
            JoinedAtUtc = joinedAtUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(joinedAtUtc, DateTimeKind.Utc)
                : joinedAtUtc.ToUniversalTime();
        }

        public AllianceMember WithRole(AllianceRole newRole)
        {
            return new AllianceMember(PlayerId, PlayerName, newRole, CombatPower, ContributionScore, JoinedAtUtc);
        }

        public bool Equals(AllianceMember other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(PlayerId, other.PlayerId, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || (obj is AllianceMember other && Equals(other));

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(PlayerId ?? string.Empty);
    }
}
