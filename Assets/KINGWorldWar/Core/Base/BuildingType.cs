namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Supported military building archetypes in the player's base.
    /// </summary>
    public enum BuildingType
    {
        /// <summary>
        /// Command Center / Headquarters (HQ) - Core progression anchor.
        /// Determines the maximum allowable upgrade level of all other buildings.
        /// </summary>
        CommandCenter = 1,

        /// <summary>
        /// Military Barracks - Infantry training, garrison capacity, and base defense.
        /// </summary>
        Barracks = 2,

        /// <summary>
        /// Supply Depot - Increases maximum capacity for Gold, Oil, and Iron storage.
        /// </summary>
        SupplyDepot = 3,

        /// <summary>
        /// Radar Station - Strategic reconnaissance, enemy scouting, and early warning intelligence.
        /// </summary>
        RadarStation = 4,

        /// <summary>
        /// Vehicle Factory - Assembly, repair, and staging of armored fighting vehicles and tanks.
        /// </summary>
        VehicleFactory = 5
    }
}
