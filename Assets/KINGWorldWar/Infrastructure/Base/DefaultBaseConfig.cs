using System.Collections.Generic;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Infrastructure.Base
{
    /// <summary>
    /// Canonical default building definitions, costs, and progression balance curves.
    /// Used by BaseConfigSO and in-memory test setups.
    /// </summary>
    public static class DefaultBaseConfig
    {
        public static IReadOnlyList<BuildingDefinition> CreateDefaultDefinitions()
        {
            return new List<BuildingDefinition>
            {
                CreateCommandCenterDefinition(),
                CreateBarracksDefinition(),
                CreateSupplyDepotDefinition(),
                CreateRadarStationDefinition(),
                CreateVehicleFactoryDefinition()
            };
        }

        public static BuildingDefinition CreateCommandCenterDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  BuildingCost.Zero,                            0,    0, 1,  "building.hq.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(500,     200,     100),      30,   0, 2,  "building.hq.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(1500,    600,     300),      60,   0, 3,  "building.hq.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(3500,    1500,    800),      120,  0, 4,  "building.hq.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(8000,    3500,    2000),     300,  0, 5,  "building.hq.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(18000,   8000,    4500),     600,  0, 6,  "building.hq.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(40000,   18000,   10000),    1200, 0, 7,  "building.hq.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(90000,   40000,   25000),    2400, 0, 8,  "building.hq.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(200000,  90000,   55000),    4800, 0, 9,  "building.hq.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(450000,  200000,  120000),   9600, 0, 10, "building.hq.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.CommandCenter,
                "building.hq.name",
                "building.hq.desc",
                footprintWidth: 4,
                footprintHeight: 4,
                levels);
        }

        public static BuildingDefinition CreateBarracksDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(200,     50,      50),       15,   1, 50,   "building.barracks.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(600,     200,     150),      45,   2, 100,  "building.barracks.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(1400,    500,     350),      90,   3, 180,  "building.barracks.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(3200,    1200,    800),      180,  4, 300,  "building.barracks.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(7500,    3000,    1800),     360,  5, 500,  "building.barracks.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(16000,   6500,    4000),     720,  6, 800,  "building.barracks.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(35000,   15000,   9000),     1500, 7, 1200, "building.barracks.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(80000,   35000,   22000),    3000, 8, 1800, "building.barracks.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(18000,   80000,   50000),    6000, 9, 2600, "building.barracks.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(400000,  180000,  110000),   12000, 10, 3600, "building.barracks.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.Barracks,
                "building.barracks.name",
                "building.barracks.desc",
                footprintWidth: 3,
                footprintHeight: 3,
                levels);
        }

        public static BuildingDefinition CreateSupplyDepotDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(300,     100,     100),      20,   1, 50000,   "building.depot.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(800,     300,     250),      50,   2, 100000,  "building.depot.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(1800,    700,     550),      100,  3, 180000,  "building.depot.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(4000,    1600,    1200),     200,  4, 300000,  "building.depot.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(9000,    3800,    2800),     400,  5, 500000,  "building.depot.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(20000,   8500,    6000),     800,  6, 800000,  "building.depot.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(45000,   19000,   14000),    1600, 7, 1200000, "building.depot.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(100000,  42000,   30000),    3200, 8, 1700000, "building.depot.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(220000,  95000,   68000),    6400, 9, 2300000, "building.depot.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(500000,  210000,  150000),   12800, 10, 3000000, "building.depot.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.SupplyDepot,
                "building.depot.name",
                "building.depot.desc",
                footprintWidth: 3,
                footprintHeight: 3,
                levels);
        }

        public static BuildingDefinition CreateRadarStationDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(250,     150,     80),       20,   1, 1,  "building.radar.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(700,     400,     220),      50,   2, 2,  "building.radar.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(1600,    900,     500),      100,  3, 3,  "building.radar.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(3600,    2100,    1100),     200,  4, 4,  "building.radar.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(8000,    4600,    2500),     400,  5, 5,  "building.radar.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(18000,   10000,   5500),     800,  6, 6,  "building.radar.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(40000,   23000,   12500),    1600, 7, 7,  "building.radar.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(90000,   50000,   28000),    3200, 8, 8,  "building.radar.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(200000,  110000,  60000),    6400, 9, 9,  "building.radar.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(450000,  250000,  135000),   12800, 10, 10, "building.radar.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.RadarStation,
                "building.radar.name",
                "building.radar.desc",
                footprintWidth: 2,
                footprintHeight: 2,
                levels);
        }

        public static BuildingDefinition CreateVehicleFactoryDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(400,     200,     150),      25,   1, 10,  "building.factory.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(1000,    500,     400),      60,   2, 20,  "building.factory.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(2200,    1100,    900),      120,  3, 35,  "building.factory.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(5000,    2500,    2000),     240,  4, 55,  "building.factory.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(11000,   5500,    4500),     480,  5, 80,  "building.factory.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(25000,   12500,   10000),    960,  6, 115, "building.factory.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(55000,   28000,   22000),    1920, 7, 160, "building.factory.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(120000,  60000,   50000),    3840, 8, 220, "building.factory.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(260000,  130000,  110000),   7680, 9, 300, "building.factory.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(600000,  300000,  250000),   15360, 10, 400, "building.factory.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.VehicleFactory,
                "building.factory.name",
                "building.factory.desc",
                footprintWidth: 4,
                footprintHeight: 3,
                levels);
        }
    }
}
