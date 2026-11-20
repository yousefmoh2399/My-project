namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Explicit classification tag for military unit statistics.
    /// Determines whether the client displays the stat as a presentation hint or respects
    /// the server-authoritative value for gameplay and combat calculations.
    /// </summary>
    public enum StatAuthority
    {
        /// <summary>
        /// Display-only presentation metadata (icons, model paths, localization keys).
        /// Non-authoritative and safe for client presentation caches.
        /// </summary>
        DisplayOnly = 1,

        /// <summary>
        /// Authoritative gameplay statistic calculated and verified strictly by the Nakama backend.
        /// Client-provided values are strictly untrusted; backend overrides in production.
        /// </summary>
        ServerAuthoritative = 2
    }
}
