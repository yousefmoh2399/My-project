namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Supported military building archetypes in the player's base.
    /// Strictly aligns with MVP Buildings (7) specified in Work Order:
    /// 1. Command Center  2. Tank Factory  3. Airfield  4. Arms Factory
    /// 5. Research Center 6. Resource Depot 7. Training Center
    /// </summary>
    public enum BuildingType
    {
        /// <summary>
        /// 1. Command Center / Headquarters (HQ) - Core progression anchor.
        /// Gates the maximum allowable tier of all other structures.
        /// </summary>
        CommandCenter = 1,

        /// <summary>
        /// 2. Tank Factory - Heavy armor manufacturing and tank maintenance.
        /// (Alias: VehicleFactory).
        /// </summary>
        TankFactory = 2,
        VehicleFactory = 2,

        /// <summary>
        /// 3. Airfield - Tactical airbase, fighter squadrons, and airspace interception.
        /// </summary>
        Airfield = 3,

        /// <summary>
        /// 4. Arms Factory - Munitions production, heavy weaponry, and explosive ordnance.
        /// </summary>
        ArmsFactory = 4,

        /// <summary>
        /// 5. Research Center - Military science, technological breakthroughs, and tactical doctrines.
        /// </summary>
        ResearchCenter = 5,

        /// <summary>
        /// 6. Resource Depot - Strategic stockpiles and secured vaults for Gold, Oil, and Iron.
        /// (Alias: SupplyDepot).
        /// </summary>
        ResourceDepot = 6,
        SupplyDepot = 6,

        /// <summary>
        /// 7. Training Center - Ground forces boot camp and tactical squad training.
        /// (Alias: Barracks).
        /// </summary>
        TrainingCenter = 7,
        Barracks = 7,

        /// <summary>
        /// Auxiliary / Legacy: Radar Station - Early warning scanning array.
        /// </summary>
        RadarStation = 8
    }
}
