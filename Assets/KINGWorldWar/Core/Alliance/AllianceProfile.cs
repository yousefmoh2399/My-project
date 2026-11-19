using System;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Immutable domain model representing an alliance's public profile and status.
    /// </summary>
    public sealed class AllianceProfile : IEquatable<AllianceProfile>
    {
        public AllianceId Id { get; }
        public string Name { get; }
        public string Tag { get; }
        public string Notice { get; }
        public string LeaderPlayerId { get; }
        public string LeaderPlayerName { get; }
        public int MemberCount { get; }
        public int MaxMembers { get; }
        public long TotalCombatPower { get; }
        public AllianceJoinPolicy JoinPolicy { get; }
        public DateTime CreatedAtUtc { get; }

        public bool IsFull => MemberCount >= MaxMembers;
        public bool CanDirectJoin => JoinPolicy == AllianceJoinPolicy.Open && !IsFull;
        public bool CanApply => JoinPolicy == AllianceJoinPolicy.ApplyOnly && !IsFull;

        public AllianceProfile(
            AllianceId id,
            string name,
            string tag,
            string notice,
            string leaderPlayerId,
            string leaderPlayerName,
            int memberCount,
            int maxMembers,
            long totalCombatPower,
            AllianceJoinPolicy joinPolicy,
            DateTime createdAtUtc)
        {
            Id = id;
            Name = name?.Trim() ?? string.Empty;
            Tag = tag?.Trim().ToUpperInvariant() ?? string.Empty;
            Notice = notice?.Trim() ?? string.Empty;
            LeaderPlayerId = leaderPlayerId ?? string.Empty;
            LeaderPlayerName = leaderPlayerName ?? "Commander";
            MemberCount = Math.Max(0, memberCount);
            MaxMembers = Math.Max(1, maxMembers);
            TotalCombatPower = Math.Max(0, totalCombatPower);
            JoinPolicy = joinPolicy;
            CreatedAtUtc = createdAtUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc)
                : createdAtUtc.ToUniversalTime();
        }

        public AllianceProfile WithMemberCount(int count, long newCombatPower)
        {
            return new AllianceProfile(
                Id, Name, Tag, Notice, LeaderPlayerId, LeaderPlayerName,
                count, MaxMembers, newCombatPower, JoinPolicy, CreatedAtUtc);
        }

        public AllianceProfile WithNotice(string newNotice)
        {
            return new AllianceProfile(
                Id, Name, Tag, newNotice, LeaderPlayerId, LeaderPlayerName,
                MemberCount, MaxMembers, TotalCombatPower, JoinPolicy, CreatedAtUtc);
        }

        public AllianceProfile WithPolicy(AllianceJoinPolicy newPolicy)
        {
            return new AllianceProfile(
                Id, Name, Tag, Notice, LeaderPlayerId, LeaderPlayerName,
                MemberCount, MaxMembers, TotalCombatPower, newPolicy, CreatedAtUtc);
        }

        public static Result<bool, GameError> ValidateCreationParameters(string name, string tag, string notice)
        {
            string trimmedName = name?.Trim() ?? string.Empty;
            if (trimmedName.Length < 3 || trimmedName.Length > 24)
            {
                return Result<bool, GameError>.Fail(
                    GameError.InvalidPayload("Alliance name must be between 3 and 24 characters."));
            }

            string trimmedTag = tag?.Trim() ?? string.Empty;
            if (trimmedTag.Length < 3 || trimmedTag.Length > 4)
            {
                return Result<bool, GameError>.Fail(
                    GameError.InvalidPayload("Alliance tag must be exactly 3 or 4 characters."));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedTag, "^[a-zA-Z0-9]+$"))
            {
                return Result<bool, GameError>.Fail(
                    GameError.InvalidPayload("Alliance tag must contain only alphanumeric characters."));
            }

            string trimmedNotice = notice?.Trim() ?? string.Empty;
            if (trimmedNotice.Length > 256)
            {
                return Result<bool, GameError>.Fail(
                    GameError.InvalidPayload("Alliance notice cannot exceed 256 characters."));
            }

            return Result<bool, GameError>.Ok(true);
        }

        public bool Equals(AllianceProfile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || (obj is AllianceProfile other && Equals(other));

        public override int GetHashCode() => Id.GetHashCode();
    }
}
