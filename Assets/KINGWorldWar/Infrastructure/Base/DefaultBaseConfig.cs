using System.Collections.Generic;
using KingWorldWar.Core.Base;

namespace KingWorldWar.Infrastructure.Base
{
    /// <summary>
    /// Canonical default building definitions, costs, and progression balance curves.
    /// Defines all 7 MVP military buildings specified in Phase 5 Work Order:
    /// 1. Command Center  2. Tank Factory  3. Airfield  4. Arms Factory
    /// 5. Research Center 6. Resource Depot 7. Training Center
    /// (plus auxiliary Radar Station for tactical radar intelligence).
    /// </summary>
    public static class DefaultBaseConfig
    {
        public static IReadOnlyList<BuildingDefinition> CreateDefaultDefinitions()
        {
            return new List<BuildingDefinition>
            {
                CreateCommandCenterDefinition(),
                CreateTankFactoryDefinition(),
                CreateAirfieldDefinition(),
                CreateArmsFactoryDefinition(),
                CreateResearchCenterDefinition(),
                CreateResourceDepotDefinition(),
                CreateTrainingCenterDefinition(),
                CreateRadarStationDefinition()
            };
        }

        /// <summary>
        /// 1. Command Center (HQ) - Core progression anchor.
        /// </summary>
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

        /// <summary>
        /// 2. Tank Factory - Heavy armor manufacturing.
        /// </summary>
        public static BuildingDefinition CreateTankFactoryDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(400,     200,     150),      25,   1, 10,  "building.tank_factory.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(1000,    500,     400),      60,   2, 20,  "building.tank_factory.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(2200,    1100,    900),      120,  3, 35,  "building.tank_factory.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(5000,    2500,    2000),     240,  4, 55,  "building.tank_factory.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(11000,   5500,    4500),     480,  5, 80,  "building.tank_factory.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(25000,   12500,   10000),    960,  6, 115, "building.tank_factory.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(55000,   28000,   22000),    1920, 7, 160, "building.tank_factory.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(120000,  60000,   50000),    3840, 8, 220, "building.tank_factory.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(260000,  130000,  110000),   7680, 9, 300, "building.tank_factory.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(600000,  300000,  250000),   15360, 10, 400, "building.tank_factory.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.TankFactory,
                "building.tank_factory.name",
                "building.tank_factory.desc",
                footprintWidth: 4,
                footprintHeight: 3,
                levels);
        }

        // Backward compatibility alias for VehicleFactory
        public static BuildingDefinition CreateVehicleFactoryDefinition() => CreateTankFactoryDefinition();

        /// <summary>
        /// 3. Airfield - Tactical military airbase & interceptor squadrons.
        /// </summary>
        public static BuildingDefinition CreateAirfieldDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(450,     250,     120),      30,   1, 1,   "building.airfield.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(1100,    600,     350),      75,   2, 2,   "building.airfield.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(2500,    1300,    800),      150,  3, 3,   "building.airfield.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(5500,    2800,    1800),     300,  4, 4,   "building.airfield.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(12000,   6200,    4000),     600,  5, 5,   "building.airfield.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(27000,   14000,   9000),     1200, 6, 6,   "building.airfield.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(60000,   31000,   20000),    2400, 7, 7,   "building.airfield.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(130000,  68000,   45000),    4800, 8, 8,   "building.airfield.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(280000,  150000,  95000),    9600, 9, 9,   "building.airfield.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(650000,  330000,  210000),   18000, 10, 10, "building.airfield.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.Airfield,
                "building.airfield.name",
                "building.airfield.desc",
                footprintWidth: 4,
                footprintHeight: 4,
                levels);
        }

        /// <summary>
        /// 4. Arms Factory - Munitions and heavy ordnance manufacturing.
        /// </summary>
        public static BuildingDefinition CreateArmsFactoryDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(350,     120,     180),      20,   1, 5,   "building.arms_factory.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(900,     350,     450),      50,   2, 10,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(2000,    800,     1000),     100,  3, 15,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(4500,    1800,    2200),     200,  4, 20,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(10000,   4000,    5000),     400,  5, 25,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(22000,   9000,    11000),    800,  6, 30,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(50000,   20000,   25000),    1600, 7, 35,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(110000,  45000,   55000),    3200, 8, 40,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(240000,  100000,  120000),   6400, 9, 45,  "building.arms_factory.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(520000,  220000,  260000),   12800, 10, 50, "building.arms_factory.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.ArmsFactory,
                "building.arms_factory.name",
                "building.arms_factory.desc",
                footprintWidth: 3,
                footprintHeight: 3,
                levels);
        }

        /// <summary>
        /// 5. Research Center - Military technology and tactical doctrines.
        /// </summary>
        public static BuildingDefinition CreateResearchCenterDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(300,     200,     100),      25,   1, 5,   "building.research_center.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(750,     500,     250),      60,   2, 10,  "building.research_center.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(1700,    1100,    550),      120,  3, 15,  "building.research_center.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(3800,    2500,    1200),     240,  4, 20,  "building.research_center.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(8500,    5500,    2800),     480,  5, 25,  "building.research_center.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(19000,   12000,   6000),     960,  6, 30,  "building.research_center.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(42000,   27000,   14000),    1920, 7, 35,  "building.research_center.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(95000,   60000,   30000),    3840, 8, 40,  "building.research_center.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(210000,  130000,  65000),    7680, 9, 45,  "building.research_center.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(480000,  300000,  150000),   15000, 10, 50, "building.research_center.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.ResearchCenter,
                "building.research_center.name",
                "building.research_center.desc",
                footprintWidth: 3,
                footprintHeight: 3,
                levels);
        }

        /// <summary>
        /// 6. Resource Depot - Protected stockpiles for Gold, Oil, and Iron.
        /// </summary>
        public static BuildingDefinition CreateResourceDepotDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(300,     100,     100),      20,   1, 50000,   "building.resource_depot.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(800,     300,     250),      50,   2, 100000,  "building.resource_depot.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(1800,    700,     550),      100,  3, 180000,  "building.resource_depot.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(4000,    1600,    1200),     200,  4, 300000,  "building.resource_depot.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(9000,    3800,    2800),     400,  5, 500000,  "building.resource_depot.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(20000,   8500,    6000),     800,  6, 800000,  "building.resource_depot.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(45000,   19000,   14000),    1600, 7, 1200000, "building.resource_depot.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(100000,  42000,   30000),    3200, 8, 1700000, "building.resource_depot.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(220000,  95000,   68000),    6400, 9, 2300000, "building.resource_depot.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(500000,  210000,  150000),   12800, 10, 3000000, "building.resource_depot.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.ResourceDepot,
                "building.resource_depot.name",
                "building.resource_depot.desc",
                footprintWidth: 3,
                footprintHeight: 3,
                levels);
        }

        // Backward compatibility alias for SupplyDepot
        public static BuildingDefinition CreateSupplyDepotDefinition() => CreateResourceDepotDefinition();

        /// <summary>
        /// 7. Training Center - Ground forces recruitment and garrison capacity.
        /// </summary>
        public static BuildingDefinition CreateTrainingCenterDefinition()
        {
            var levels = new List<BuildingLevelData>
            {
                new BuildingLevelData(1,  new BuildingCost(200,     50,      50),       15,   1, 50,   "building.training_center.desc_perk"),
                new BuildingLevelData(2,  new BuildingCost(600,     200,     150),      45,   2, 100,  "building.training_center.desc_perk"),
                new BuildingLevelData(3,  new BuildingCost(1400,    500,     350),      90,   3, 180,  "building.training_center.desc_perk"),
                new BuildingLevelData(4,  new BuildingCost(3200,    1200,    800),      180,  4, 300,  "building.training_center.desc_perk"),
                new BuildingLevelData(5,  new BuildingCost(7500,    3000,    1800),     360,  5, 500,  "building.training_center.desc_perk"),
                new BuildingLevelData(6,  new BuildingCost(16000,   6500,    4000),     720,  6, 800,  "building.training_center.desc_perk"),
                new BuildingLevelData(7,  new BuildingCost(35000,   15000,   9000),     1500, 7, 1200, "building.training_center.desc_perk"),
                new BuildingLevelData(8,  new BuildingCost(80000,   35000,   22000),    3000, 8, 1800, "building.training_center.desc_perk"),
                new BuildingLevelData(9,  new BuildingCost(18000,   80000,   50000),    6000, 9, 2600, "building.training_center.desc_perk"),
                new BuildingLevelData(10, new BuildingCost(400000,  180000,  110000),   12000, 10, 3600, "building.training_center.desc_perk")
            };

            return new BuildingDefinition(
                BuildingType.TrainingCenter,
                "building.training_center.name",
                "building.training_center.desc",
                footprintWidth: 3,
                footprintHeight: 3,
                levels);
        }

        // Backward compatibility alias for Barracks
        public static BuildingDefinition CreateBarracksDefinition() => CreateTrainingCenterDefinition();

        /// <summary>
        /// Auxiliary / Legacy: Radar Station - Early warning scanning array.
        /// </summary>
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
    }
}
