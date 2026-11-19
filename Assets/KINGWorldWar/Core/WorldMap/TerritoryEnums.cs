namespace KingWorldWar.Core.WorldMap
{
    public enum TerritoryType
    {
        RegionalCapital,
        Fortress,
        Chokepoint,
        ResourceHub,
        Outpost,
        Plains,
        Frontier
    }

    /// <summary>
    /// Faction alignment for territory ownership display.
    /// Provides accessibility symbols for colorblind players (WCAG AAA).
    /// </summary>
    public enum FactionType
    {
        Neutral = 0,
        PlayerAllied = 1,
        HostileRed = 2,
        HostileBlue = 3
    }

    public static class FactionAccessibilityExtensions
    {
        /// <summary>
        /// Distinct geometric symbol associated with each faction for colorblind readability.
        /// </summary>
        public static string GetSymbol(this FactionType faction)
        {
            switch (faction)
            {
                case FactionType.PlayerAllied: return "★"; // Star
                case FactionType.Neutral: return "◆";      // Diamond
                case FactionType.HostileRed: return "✖";    // Cross
                case FactionType.HostileBlue: return "▲";   // Triangle
                default: return "●";
            }
        }

        public static string GetLocalizationKey(this FactionType faction)
        {
            switch (faction)
            {
                case FactionType.PlayerAllied: return "faction.allied";
                case FactionType.Neutral: return "faction.neutral";
                case FactionType.HostileRed: return "faction.hostile_red";
                case FactionType.HostileBlue: return "faction.hostile_blue";
                default: return "faction.unknown";
            }
        }
    }
}
