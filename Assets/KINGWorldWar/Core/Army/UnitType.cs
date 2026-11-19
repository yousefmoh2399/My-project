namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Supported military unit archetypes in KING WORLD WAR.
    /// </summary>
    public enum UnitType
    {
        /// <summary>
        /// Frontline tactical infantry squad. Trained in Barracks.
        /// High defensive garrison and urban defense rating.
        /// </summary>
        Infantry = 1,

        /// <summary>
        /// Main battle tank / heavy armored fighting vehicle. Built in Vehicle Factory.
        /// Primary assault armor with high direct firepower and frontal protection.
        /// </summary>
        Tank = 2,

        /// <summary>
        /// Long-range self-propelled artillery. Built in Vehicle Factory.
        /// High area-of-effect damage, extended range, low armor.
        /// </summary>
        Artillery = 3,

        /// <summary>
        /// High-speed light armored reconnaissance vehicle. Built in Barracks / Factory.
        /// High mobility, strategic speed, and tactical reconnaissance.
        /// </summary>
        ReconVehicle = 4
    }
}
