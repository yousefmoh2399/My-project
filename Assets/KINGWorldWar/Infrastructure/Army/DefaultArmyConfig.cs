using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Infrastructure.Army
{
    /// <summary>
    /// Static baseline configuration providing canonical military unit definitions for KING WORLD WAR.
    /// Balanced across 4 archetypes with progression prerequisites tied to player base structures.
    /// </summary>
    public static class DefaultArmyConfig
    {
        public static readonly UnitId InfantryId = new UnitId("unt_infantry");
        public static readonly UnitId ReconId = new UnitId("unt_recon");
        public static readonly UnitId TankId = new UnitId("unt_tank");
        public static readonly UnitId ArtilleryId = new UnitId("unt_artillery");

        public static List<UnitDefinition> CreateDefaultDefinitions()
        {
            return new List<UnitDefinition>
            {
                // 1. Tactical Infantry (Light Infantry Squad)
                // Prerequisite: Barracks Level 1
                new UnitDefinition(
                    id: InfantryId,
                    type: UnitType.Infantry,
                    nameKey: "unit.infantry.name",
                    descKey: "unit.infantry.desc",
                    roleKey: "unit.infantry.role",
                    stats: new UnitStats(
                        health: 120,
                        attack: 25,
                        defense: 15,
                        speed: 6.0f,
                        range: 15.0f,
                        combatPower: 40),
                    cost: new UnitCost(
                        gold: 50,
                        oil: 10,
                        iron: 20,
                        recruitDurationSeconds: 15),
                    requiredBuildingType: BuildingType.Barracks,
                    requiredBuildingLevel: 1,
                    batchSize: 10),

                // 2. Armored Recon Vehicle (Fast Scout)
                // Prerequisite: Barracks Level 2
                new UnitDefinition(
                    id: ReconId,
                    type: UnitType.ReconVehicle,
                    nameKey: "unit.recon.name",
                    descKey: "unit.recon.desc",
                    roleKey: "unit.recon.role",
                    stats: new UnitStats(
                        health: 350,
                        attack: 60,
                        defense: 40,
                        speed: 12.0f,
                        range: 25.0f,
                        combatPower: 110),
                    cost: new UnitCost(
                        gold: 150,
                        oil: 80,
                        iron: 70,
                        recruitDurationSeconds: 30),
                    requiredBuildingType: BuildingType.Barracks,
                    requiredBuildingLevel: 2,
                    batchSize: 1),

                // 3. Main Battle Tank (Heavy Armor Assault)
                // Prerequisite: Vehicle Factory Level 1
                new UnitDefinition(
                    id: TankId,
                    type: UnitType.Tank,
                    nameKey: "unit.tank.name",
                    descKey: "unit.tank.desc",
                    roleKey: "unit.tank.role",
                    stats: new UnitStats(
                        health: 850,
                        attack: 160,
                        defense: 120,
                        speed: 7.0f,
                        range: 30.0f,
                        combatPower: 320),
                    cost: new UnitCost(
                        gold: 350,
                        oil: 200,
                        iron: 250,
                        recruitDurationSeconds: 60),
                    requiredBuildingType: BuildingType.VehicleFactory,
                    requiredBuildingLevel: 1,
                    batchSize: 1),

                // 4. Heavy Artillery (Long-Range Siege Howitzer)
                // Prerequisite: Vehicle Factory Level 2
                new UnitDefinition(
                    id: ArtilleryId,
                    type: UnitType.Artillery,
                    nameKey: "unit.artillery.name",
                    descKey: "unit.artillery.desc",
                    roleKey: "unit.artillery.role",
                    stats: new UnitStats(
                        health: 500,
                        attack: 280,
                        defense: 50,
                        speed: 4.5f,
                        range: 55.0f,
                        combatPower: 380),
                    cost: new UnitCost(
                        gold: 500,
                        oil: 300,
                        iron: 400,
                        recruitDurationSeconds: 90),
                    requiredBuildingType: BuildingType.VehicleFactory,
                    requiredBuildingLevel: 2,
                    batchSize: 1)
            };
        }
    }
}
