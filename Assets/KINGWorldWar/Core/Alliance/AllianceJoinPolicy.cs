namespace KingWorldWar.Core.Alliance
{
    /// <summary>
    /// Membership admission policies for an alliance.
    /// </summary>
    public enum AllianceJoinPolicy
    {
        Open = 1,       // Any player can join immediately without approval
        ApplyOnly = 2,  // Requires officer/leader approval of submitted application
        Closed = 3      // No new members accepted
    }
}
