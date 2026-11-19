using System;

namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Hierarchy of member roles within a military alliance.
    /// </summary>
    public enum AllianceRole
    {
        None = 0,
        Leader = 1,
        Officer = 2,
        Veteran = 3,
        Member = 4,
        Recruit = 5
    }

    /// <summary>
    /// Strict permission evaluation matrix based on military role hierarchy.
    /// Client-side enforcement mirrors server authoritative validation rules.
    /// </summary>
    public static class AllianceRoleExtensions
    {
        public static bool CanInvite(this AllianceRole role) =>
            role == AllianceRole.Leader || role == AllianceRole.Officer;

        public static bool CanManageApplications(this AllianceRole role) =>
            role == AllianceRole.Leader || role == AllianceRole.Officer;

        public static bool CanEditNotice(this AllianceRole role) =>
            role == AllianceRole.Leader || role == AllianceRole.Officer;

        public static bool CanDisband(this AllianceRole role) =>
            role == AllianceRole.Leader;

        public static bool CanTransferLeadership(this AllianceRole role) =>
            role == AllianceRole.Leader;

        public static bool CanKick(this AllianceRole actorRole, AllianceRole targetRole)
        {
            if (actorRole == AllianceRole.None || targetRole == AllianceRole.None) return false;
            if (actorRole == targetRole) return false; // Cannot kick self via kick command

            if (actorRole == AllianceRole.Leader)
            {
                return true; // Leader can kick anyone
            }

            if (actorRole == AllianceRole.Officer)
            {
                // Officers can only kick lower ranks (Veteran, Member, Recruit)
                return targetRole == AllianceRole.Veteran ||
                       targetRole == AllianceRole.Member ||
                       targetRole == AllianceRole.Recruit;
            }

            return false;
        }

        public static bool CanPromote(this AllianceRole actorRole, AllianceRole currentTargetRole, AllianceRole targetNewRole)
        {
            if (actorRole == AllianceRole.None) return false;
            if (actorRole != AllianceRole.Leader && actorRole != AllianceRole.Officer) return false;

            // Only Leader can promote to Officer or Leader
            if (targetNewRole == AllianceRole.Leader || targetNewRole == AllianceRole.Officer)
            {
                return actorRole == AllianceRole.Leader;
            }

            // Officer can only promote Member/Recruit to Veteran or Member
            if (actorRole == AllianceRole.Officer)
            {
                if (currentTargetRole != AllianceRole.Member && currentTargetRole != AllianceRole.Recruit) return false;
                return targetNewRole == AllianceRole.Veteran || targetNewRole == AllianceRole.Member;
            }

            return true;
        }

        public static bool CanDemote(this AllianceRole actorRole, AllianceRole currentTargetRole, AllianceRole targetNewRole)
        {
            if (actorRole == AllianceRole.None) return false;
            if (actorRole != AllianceRole.Leader && actorRole != AllianceRole.Officer) return false;

            // Only Leader can demote Officers
            if (currentTargetRole == AllianceRole.Officer)
            {
                return actorRole == AllianceRole.Leader;
            }

            // Officer can only demote Veterans to Member or Recruit
            if (actorRole == AllianceRole.Officer)
            {
                if (currentTargetRole != AllianceRole.Veteran && currentTargetRole != AllianceRole.Member) return false;
                return targetNewRole == AllianceRole.Member || targetNewRole == AllianceRole.Recruit;
            }

            return true;
        }
    }
}
