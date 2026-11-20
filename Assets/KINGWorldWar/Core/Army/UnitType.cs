namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// The 5 MVP military unit archetypes in KING WORLD WAR.
    /// Classified into Ground and Air operational categories.
    /// </summary>
    public enum UnitType
    {
        /// <summary>
        /// Ground: Main battle tank / heavy armored fighting vehicle. Built in Tank Factory.
        /// Primary assault armor with high direct firepower and frontal protection.
        /// </summary>
        Tank = 1,

        /// <summary>
        /// Ground: Mechanized infantry fighting vehicle / rapid assault carrier. Built in Tank Factory.
        /// High mobility, infantry transport capacity, and versatile autocannon support.
        /// </summary>
        ArmoredVehicle = 2,

        /// <summary>
        /// Ground: Long-range self-propelled heavy artillery. Built in Tank Factory / Arms Factory.
        /// High area-of-effect bombardment damage, extended firing range, light defensive armor.
        /// </summary>
        Artillery = 3,

        /// <summary>
        /// Air: High-speed tactical air superiority fighter. Built in Airfield.
        /// Intercepts enemy aircraft, provides rapid close air support and air-to-ground strikes.
        /// </summary>
        FighterAircraft = 4,

        /// <summary>
        /// Air: Unmanned / light reconnaissance scout aircraft. Built in Airfield.
        /// Extended surveillance radius, high speed, unveils fog of war and enemy territory garrisons.
        /// </summary>
        ReconAircraft = 5,

        /// <summary>
        /// Deprecated legacy archetype from Phase 6 prototype.
        /// </summary>
        [System.Obsolete("Deprecated in Phase 7 MVP. Use ArmoredVehicle or Tank.")]
        Infantry = 6,

        /// <summary>
        /// Deprecated legacy archetype from Phase 6 prototype.
        /// </summary>
        [System.Obsolete("Deprecated in Phase 7 MVP. Use ArmoredVehicle or ReconAircraft.")]
        ReconVehicle = 7
    }
}
