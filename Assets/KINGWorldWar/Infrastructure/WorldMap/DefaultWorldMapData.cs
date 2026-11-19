using System.Collections.Generic;
using KingWorldWar.Core.WorldMap;
using UnityEngine;

namespace KingWorldWar.Infrastructure.WorldMap
{
    /// <summary>
    /// Factory providing canonical default world map data (5 regions, 25 territories).
    /// Used for initial ScriptableObject seeding, testing, and offline dev fallback.
    /// Non-authoritative: Actual ownership is synchronized from Nakama server.
    /// </summary>
    public static class DefaultWorldMapData
    {
        public static readonly RegionId RegionNorth = new RegionId("reg_north");
        public static readonly RegionId RegionCoast = new RegionId("reg_coast");
        public static readonly RegionId RegionOasis = new RegionId("reg_oasis");
        public static readonly RegionId RegionValley = new RegionId("reg_valley");
        public static readonly RegionId RegionRidge = new RegionId("reg_ridge");

        public static RegionData[] CreateDefaultRegions()
        {
            return new[]
            {
                new RegionData(
                    RegionNorth,
                    "region.north.name",
                    "region.north.desc",
                    "Iron",
                    new Color32(0x4A, 0x55, 0x68, 0xFF) // Steel / Frost
                ),
                new RegionData(
                    RegionCoast,
                    "region.coast.name",
                    "region.coast.desc",
                    "Oil",
                    new Color32(0x3B, 0x6E, 0x8C, 0xFF) // Deep Navy Teal
                ),
                new RegionData(
                    RegionOasis,
                    "region.oasis.name",
                    "region.oasis.desc",
                    "Oil & Gold",
                    new Color32(0xD4, 0xAF, 0x37, 0xFF) // Desert Gold
                ),
                new RegionData(
                    RegionValley,
                    "region.valley.name",
                    "region.valley.desc",
                    "Gold & Supplies",
                    new Color32(0x3E, 0x4E, 0x3D, 0xFF) // Military Green
                ),
                new RegionData(
                    RegionRidge,
                    "region.ridge.name",
                    "region.ridge.desc",
                    "Iron & Ordnance",
                    new Color32(0x5A, 0x2A, 0x27, 0xFF) // Volcanic Rust / Obsidian
                )
            };
        }

        public static TerritoryData[] CreateDefaultTerritories()
        {
            return new[]
            {
                // ==================== 1. NORTH REGION (reg_north) ====================
                new TerritoryData(
                    new TerritoryId("ter_nrd_001"), RegionNorth,
                    "territory.nrd_001.name", "territory.nrd_001.desc",
                    TerritoryType.RegionalCapital,
                    new Vector3(0f, 0f, 90f),
                    new ResourceYield(500, 200, 1800), 4500,
                    new[] { "ter_nrd_002", "ter_nrd_003" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_nrd_002"), RegionNorth,
                    "territory.nrd_002.name", "territory.nrd_002.desc",
                    TerritoryType.Chokepoint,
                    new Vector3(-25f, 0f, 70f),
                    new ResourceYield(300, 150, 1200), 3000,
                    new[] { "ter_nrd_001", "ter_nrd_004", "ter_val_001" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_nrd_003"), RegionNorth,
                    "territory.nrd_003.name", "territory.nrd_003.desc",
                    TerritoryType.ResourceHub,
                    new Vector3(25f, 0f, 70f),
                    new ResourceYield(400, 100, 2200), 2800,
                    new[] { "ter_nrd_001", "ter_nrd_005", "ter_rdg_002" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_nrd_004"), RegionNorth,
                    "territory.nrd_004.name", "territory.nrd_004.desc",
                    TerritoryType.Outpost,
                    new Vector3(-50f, 0f, 60f),
                    new ResourceYield(250, 100, 1000), 2200,
                    new[] { "ter_nrd_002", "ter_nrd_005", "ter_cst_005" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_nrd_005"), RegionNorth,
                    "territory.nrd_005.name", "territory.nrd_005.desc",
                    TerritoryType.Plains,
                    new Vector3(0f, 0f, 55f),
                    new ResourceYield(350, 250, 1100), 2500,
                    new[] { "ter_nrd_003", "ter_nrd_004", "ter_val_004" }
                ),

                // ==================== 2. COAST REGION (reg_coast) ====================
                new TerritoryData(
                    new TerritoryId("ter_cst_001"), RegionCoast,
                    "territory.cst_001.name", "territory.cst_001.desc",
                    TerritoryType.RegionalCapital,
                    new Vector3(-85f, 0f, -20f),
                    new ResourceYield(800, 1600, 600), 4200,
                    new[] { "ter_cst_002", "ter_cst_003" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_cst_002"), RegionCoast,
                    "territory.cst_002.name", "territory.cst_002.desc",
                    TerritoryType.ResourceHub,
                    new Vector3(-65f, 0f, 0f),
                    new ResourceYield(600, 2000, 500), 3200,
                    new[] { "ter_cst_001", "ter_cst_004", "ter_val_002" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_cst_003"), RegionCoast,
                    "territory.cst_003.name", "territory.cst_003.desc",
                    TerritoryType.Chokepoint,
                    new Vector3(-70f, 0f, -45f),
                    new ResourceYield(500, 1200, 700), 3100,
                    new[] { "ter_cst_001", "ter_cst_004", "ter_rdg_005" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_cst_004"), RegionCoast,
                    "territory.cst_004.name", "territory.cst_004.desc",
                    TerritoryType.Outpost,
                    new Vector3(-50f, 0f, -25f),
                    new ResourceYield(450, 1400, 550), 2400,
                    new[] { "ter_cst_002", "ter_cst_003", "ter_cst_005" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_cst_005"), RegionCoast,
                    "territory.cst_005.name", "territory.cst_005.desc",
                    TerritoryType.Fortress,
                    new Vector3(-55f, 0f, 25f),
                    new ResourceYield(400, 900, 800), 3500,
                    new[] { "ter_cst_004", "ter_nrd_004", "ter_val_004" }
                ),

                // ==================== 3. OASIS REGION (reg_oasis) ====================
                new TerritoryData(
                    new TerritoryId("ter_oas_001"), RegionOasis,
                    "territory.oas_001.name", "territory.oas_001.desc",
                    TerritoryType.RegionalCapital,
                    new Vector3(85f, 0f, -20f),
                    new ResourceYield(1200, 1800, 400), 4600,
                    new[] { "ter_oas_002", "ter_oas_003" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_oas_002"), RegionOasis,
                    "territory.oas_002.name", "territory.oas_002.desc",
                    TerritoryType.Plains,
                    new Vector3(65f, 0f, 5f),
                    new ResourceYield(900, 1500, 300), 2700,
                    new[] { "ter_oas_001", "ter_oas_004", "ter_val_003" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_oas_003"), RegionOasis,
                    "territory.oas_003.name", "territory.oas_003.desc",
                    TerritoryType.ResourceHub,
                    new Vector3(70f, 0f, -45f),
                    new ResourceYield(600, 2400, 350), 3400,
                    new[] { "ter_oas_001", "ter_oas_005", "ter_rdg_004" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_oas_004"), RegionOasis,
                    "territory.oas_004.name", "territory.oas_004.desc",
                    TerritoryType.Chokepoint,
                    new Vector3(45f, 0f, -10f),
                    new ResourceYield(700, 1100, 500), 3200,
                    new[] { "ter_oas_002", "ter_oas_005", "ter_val_005" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_oas_005"), RegionOasis,
                    "territory.oas_005.name", "territory.oas_005.desc",
                    TerritoryType.Outpost,
                    new Vector3(50f, 0f, -40f),
                    new ResourceYield(550, 1300, 450), 2300,
                    new[] { "ter_oas_003", "ter_oas_004", "ter_rdg_003" }
                ),

                // ==================== 4. VALLEY REGION (reg_valley) ====================
                new TerritoryData(
                    new TerritoryId("ter_val_001"), RegionValley,
                    "territory.val_001.name", "territory.val_001.desc",
                    TerritoryType.RegionalCapital,
                    new Vector3(0f, 0f, 20f),
                    new ResourceYield(2000, 800, 800), 5000,
                    new[] { "ter_val_002", "ter_val_003", "ter_val_004", "ter_nrd_002" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_val_002"), RegionValley,
                    "territory.val_002.name", "territory.val_002.desc",
                    TerritoryType.Chokepoint,
                    new Vector3(-25f, 0f, 0f),
                    new ResourceYield(1100, 700, 600), 3300,
                    new[] { "ter_val_001", "ter_val_005", "ter_cst_002" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_val_003"), RegionValley,
                    "territory.val_003.name", "territory.val_003.desc",
                    TerritoryType.ResourceHub,
                    new Vector3(25f, 0f, 0f),
                    new ResourceYield(1800, 600, 500), 3000,
                    new[] { "ter_val_001", "ter_val_005", "ter_oas_002" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_val_004"), RegionValley,
                    "territory.val_004.name", "territory.val_004.desc",
                    TerritoryType.Fortress,
                    new Vector3(-25f, 0f, 35f),
                    new ResourceYield(900, 500, 900), 3800,
                    new[] { "ter_val_001", "ter_nrd_005", "ter_cst_005" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_val_005"), RegionValley,
                    "territory.val_005.name", "territory.val_005.desc",
                    TerritoryType.Plains,
                    new Vector3(0f, 0f, -10f),
                    new ResourceYield(1300, 650, 700), 2600,
                    new[] { "ter_val_002", "ter_val_003", "ter_oas_004" }
                ),

                // ==================== 5. RIDGE REGION (reg_ridge) ====================
                new TerritoryData(
                    new TerritoryId("ter_rdg_001"), RegionRidge,
                    "territory.rdg_001.name", "territory.rdg_001.desc",
                    TerritoryType.RegionalCapital,
                    new Vector3(0f, 0f, -80f),
                    new ResourceYield(700, 600, 2200), 4800,
                    new[] { "ter_rdg_002", "ter_rdg_003" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_rdg_002"), RegionRidge,
                    "territory.rdg_002.name", "territory.rdg_002.desc",
                    TerritoryType.Chokepoint,
                    new Vector3(20f, 0f, -65f),
                    new ResourceYield(500, 400, 1600), 3400,
                    new[] { "ter_rdg_001", "ter_rdg_004", "ter_nrd_003" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_rdg_003"), RegionRidge,
                    "territory.rdg_003.name", "territory.rdg_003.desc",
                    TerritoryType.ResourceHub,
                    new Vector3(-20f, 0f, -65f),
                    new ResourceYield(450, 500, 1900), 3100,
                    new[] { "ter_rdg_001", "ter_rdg_005", "ter_oas_005" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_rdg_004"), RegionRidge,
                    "territory.rdg_004.name", "territory.rdg_004.desc",
                    TerritoryType.Plains,
                    new Vector3(30f, 0f, -85f),
                    new ResourceYield(400, 550, 1400), 2500,
                    new[] { "ter_rdg_002", "ter_rdg_005", "ter_oas_003" }
                ),
                new TerritoryData(
                    new TerritoryId("ter_rdg_005"), RegionRidge,
                    "territory.rdg_005.name", "territory.rdg_005.desc",
                    TerritoryType.Fortress,
                    new Vector3(-30f, 0f, -85f),
                    new ResourceYield(400, 450, 1800), 3900,
                    new[] { "ter_rdg_003", "ter_rdg_004", "ter_cst_003" }
                )
            };
        }
    }
}
