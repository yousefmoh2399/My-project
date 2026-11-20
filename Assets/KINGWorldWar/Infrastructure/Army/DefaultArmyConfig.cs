using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Infrastructure.Army
{
    /// <summary>
    /// Static baseline configuration providing canonical military unit definitions for KING WORLD WAR.
    /// Encapsulates all 5 MVP unit types across Ground and Air operational categories.
    /// </summary>
    public static class DefaultArmyConfig
    {
        public static readonly UnitId TankId = new UnitId("unt_tank");
        public static readonly UnitId ArmoredVehicleId = new UnitId("unt_armored_vehicle");
        public static readonly UnitId ArtilleryId = new UnitId("unt_artillery");
        public static readonly UnitId FighterAircraftId = new UnitId("unt_fighter_aircraft");
        public static readonly UnitId ReconAircraftId = new UnitId("unt_recon_aircraft");

        public static List<UnitDefinition> CreateDefaultDefinitions()
        {
            return new List<UnitDefinition>
            {
                // 1. Main Battle Tank (Heavy Armor Assault) - Ground
                // Prerequisite: Tank Factory Level 1
                new UnitDefinition(
                    id: TankId,
                    type: UnitType.Tank,
                    category: UnitCategory.Ground,
                    nameKey: "unit.tank.name",
                    descKey: "unit.tank.desc",
                    roleKey: "unit.tank.role",
                    stats: new UnitStats(
                        health: 850,
                        attack: 160,
                        defense: 120,
                        speed: 7.0f,
                        range: 30.0f,
                        capacityWeight: 3,
                        combatPower: 320),
                    cost: new UnitCost(
                        gold: 350,
                        oil: 200,
                        iron: 250,
                        recruitDurationSeconds: 60),
                    requiredBuildingType: BuildingType.TankFactory,
                    requiredBuildingLevel: 1,
                    tier: 1,
                    batchSize: 1,
                    iconAssetPath: "Icons/Units/ic_unit_tank",
                    modelAssetPath: "Assets/KINGWorldWar/Art/Prefabs/Units/Unit_Tank_Placeholder.prefab",
                    nextLevelBenefitsKey: "unit.tank.next_perk",
                    nextLevelUpgradeCost: new UnitCost(5000, 3000, 3000, 180),
                    nextLevelUpgradeDurationSeconds: 180),

                // 2. Armored Vehicle (Mechanized Support Carrier) - Ground
                // Prerequisite: Tank Factory Level 1
                new UnitDefinition(
                    id: ArmoredVehicleId,
                    type: UnitType.ArmoredVehicle,
                    category: UnitCategory.Ground,
                    nameKey: "unit.armored_vehicle.name",
                    descKey: "unit.armored_vehicle.desc",
                    roleKey: "unit.armored_vehicle.role",
                    stats: new UnitStats(
                        health: 450,
                        attack: 85,
                        defense: 60,
                        speed: 12.0f,
                        range: 22.0f,
                        capacityWeight: 2,
                        combatPower: 160),
                    cost: new UnitCost(
                        gold: 200,
                        oil: 120,
                        iron: 100,
                        recruitDurationSeconds: 35),
                    requiredBuildingType: BuildingType.TankFactory,
                    requiredBuildingLevel: 1,
                    tier: 1,
                    batchSize: 1,
                    iconAssetPath: "Icons/Units/ic_unit_armored_vehicle",
                    modelAssetPath: "Assets/KINGWorldWar/Art/Prefabs/Units/Unit_ArmoredVehicle_Placeholder.prefab",
                    nextLevelBenefitsKey: "unit.armored_vehicle.next_perk",
                    nextLevelUpgradeCost: new UnitCost(3500, 2000, 2000, 120),
                    nextLevelUpgradeDurationSeconds: 120),

                // 3. Heavy Artillery (Long-Range Siege Howitzer) - Ground
                // Prerequisite: Tank Factory Level 2
                new UnitDefinition(
                    id: ArtilleryId,
                    type: UnitType.Artillery,
                    category: UnitCategory.Ground,
                    nameKey: "unit.artillery.name",
                    descKey: "unit.artillery.desc",
                    roleKey: "unit.artillery.role",
                    stats: new UnitStats(
                        health: 380,
                        attack: 220,
                        defense: 45,
                        speed: 5.0f,
                        range: 55.0f,
                        capacityWeight: 3,
                        combatPower: 280),
                    cost: new UnitCost(
                        gold: 400,
                        oil: 150,
                        iron: 350,
                        recruitDurationSeconds: 75),
                    requiredBuildingType: BuildingType.TankFactory,
                    requiredBuildingLevel: 2,
                    tier: 1,
                    batchSize: 1,
                    iconAssetPath: "Icons/Units/ic_unit_artillery",
                    modelAssetPath: "Assets/KINGWorldWar/Art/Prefabs/Units/Unit_Artillery_Placeholder.prefab",
                    nextLevelBenefitsKey: "unit.artillery.next_perk",
                    nextLevelUpgradeCost: new UnitCost(6000, 3000, 4500, 240),
                    nextLevelUpgradeDurationSeconds: 240),

                // 4. Fighter Aircraft (Air Superiority Interceptor) - Air
                // Prerequisite: Airfield Level 1
                new UnitDefinition(
                    id: FighterAircraftId,
                    type: UnitType.FighterAircraft,
                    category: UnitCategory.Air,
                    nameKey: "unit.fighter_aircraft.name",
                    descKey: "unit.fighter_aircraft.desc",
                    roleKey: "unit.fighter_aircraft.role",
                    stats: new UnitStats(
                        health: 500,
                        attack: 240,
                        defense: 80,
                        speed: 22.0f,
                        range: 40.0f,
                        capacityWeight: 4,
                        combatPower: 350),
                    cost: new UnitCost(
                        gold: 500,
                        oil: 400,
                        iron: 300,
                        recruitDurationSeconds: 90),
                    requiredBuildingType: BuildingType.Airfield,
                    requiredBuildingLevel: 1,
                    tier: 1,
                    batchSize: 1,
                    iconAssetPath: "Icons/Units/ic_unit_fighter",
                    modelAssetPath: "Assets/KINGWorldWar/Art/Prefabs/Units/Unit_FighterAircraft_Placeholder.prefab",
                    nextLevelBenefitsKey: "unit.fighter_aircraft.next_perk",
                    nextLevelUpgradeCost: new UnitCost(8000, 6000, 5000, 300),
                    nextLevelUpgradeDurationSeconds: 300),

                // 5. Recon Aircraft (Strategic Aerial Scout) - Air
                // Prerequisite: Airfield Level 1
                new UnitDefinition(
                    id: ReconAircraftId,
                    type: UnitType.ReconAircraft,
                    category: UnitCategory.Air,
                    nameKey: "unit.recon_aircraft.name",
                    descKey: "unit.recon_aircraft.desc",
                    roleKey: "unit.recon_aircraft.role",
                    stats: new UnitStats(
                        health: 280,
                        attack: 50,
                        defense: 40,
                        speed: 28.0f,
                        range: 70.0f,
                        capacityWeight: 2,
                        combatPower: 120),
                    cost: new UnitCost(
                        gold: 250,
                        oil: 250,
                        iron: 120,
                        recruitDurationSeconds: 40),
                    requiredBuildingType: BuildingType.Airfield,
                    requiredBuildingLevel: 1,
                    tier: 1,
                    batchSize: 1,
                    iconAssetPath: "Icons/Units/ic_unit_recon_air",
                    modelAssetPath: "Assets/KINGWorldWar/Art/Prefabs/Units/Unit_ReconAircraft_Placeholder.prefab",
                    nextLevelBenefitsKey: "unit.recon_aircraft.next_perk",
                    nextLevelUpgradeCost: new UnitCost(4000, 3500, 2000, 150),
                    nextLevelUpgradeDurationSeconds: 150)
            };
        }
    }
}
