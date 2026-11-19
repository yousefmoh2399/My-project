# How to Add New Regions and Territories in KING WORLD WAR

This document provides complete, step-by-step instructions for game designers and engineers to add new regions and territories to KING WORLD WAR without modifying core gameplay logic or breaking graph topology.

---

## 1. Overview of the World Map Architecture

The world map architecture is strictly data-driven, adhering to the pure C# domain abstractions defined in `KingWorldWar.Core.WorldMap`:

- **`RegionId` & `RegionData`**: Unique identifier (`reg_*`) and region metadata (display name key, theme color, capital territory ID).
- **`TerritoryId` & `TerritoryData`**: Unique identifier (`ter_*`), containing:
  - `RegionId`: Parent region reference.
  - `NameKey` / `DescriptionKey`: Localization string table keys.
  - `TerritoryType`: `Capital`, `ResourceHub`, `Fortress`, `Frontier`, or `Standard`.
  - `DefaultYield`: Base hourly production (`GoldPerHour`, `OilPerHour`, `IronPerHour`).
  - `MapCoordinates`: Normalized 2D world position $(X, Y)$ mapped onto the 3D ground plane $(X, 0, Y)$.
  - `Neighbors`: Set of connected adjacent `TerritoryId`s.
- **`TerritoryGraph`**: Pure C# mathematical graph that validates bidirectional adjacency symmetry, graph connectivity, and calculates shortest paths using Breadth-First Search (BFS).
- **`WorldMapConfigSO`**: The Unity `ScriptableObject` implementing `ITerritoryDataSource`, serializing the static data.
- **Server Authority**: Territory yields and defense ratings displayed by the client are strictly non-authoritative until synchronized with Nakama server RPCs in Phase 8/9.

---

## 2. Step-by-Step: Adding a New Territory

### Step 2.1: Assign Stable Identifiers
1. **Territory ID**: Follow the pattern `ter_<region_code>_<001-999>`.
   - Example: For a new territory in the Coast region, use `ter_cst_006`.
2. **Localization Keys**:
   - Name Key: `territory.ter_cst_006.name`
   - Description Key: `territory.ter_cst_006.desc`

### Step 2.2: Add Localization Strings
Open `Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs` (or your external localization sheet) and add both English and Arabic entries with 100% key parity:

```csharp
// English
{ "territory.ter_cst_006.name", "New Port Bastion" },
{ "territory.ter_cst_006.desc", "Strategic coastal bastion guarding oceanic trade." },

// Arabic (RTL shaped automatically by ArabicTextShaper)
{ "territory.ter_cst_006.name", "معقل الميناء الجديد" },
{ "territory.ter_cst_006.desc", "معقل ساحلي استراتيجي يحرس التجارة البحرية." },
```
*Remember to add the Arabic strings to `docs/localization/REVIEW_QUEUE.md` for human review.*

### Step 2.3: Define Territory Data & Symmetric Neighbors
In `DefaultWorldMapData.cs` (or within the `WorldMapConfigSO` inspector in Unity):

1. Instantiate the `TerritoryData`:
```csharp
var newTerritory = new TerritoryData(
    new TerritoryId("ter_cst_006"),
    new RegionId("reg_coast"),
    "territory.ter_cst_006.name",
    "territory.ter_cst_006.desc",
    TerritoryType.Fortress,
    new ResourceYield(goldPerHour: 120, oilPerHour: 180, ironPerHour: 90),
    new Vector2(25.0f, -10.0f)
);
```

2. **Crucial Rule — Bidirectional Adjacency**:
Every connection in `TerritoryGraph` MUST be bidirectional. If territory $A$ connects to $B$, territory $B$ MUST connect to $A$.
```csharp
// Connect symmetrically
newTerritory.AddNeighbor(new TerritoryId("ter_cst_001"));
coastTerritory001.AddNeighbor(new TerritoryId("ter_cst_006"));
```

---

## 3. Step-by-Step: Adding a New Region

To add a 6th region (e.g. `reg_archipelago`):

1. **Define Region ID & Model**:
```csharp
var archipelago = new RegionData(
    new RegionId("reg_archipelago"),
    "region.reg_archipelago.name",
    new Color(0.2f, 0.6f, 0.8f, 1f),
    new TerritoryId("ter_arc_001") // Initial designated capital
);
```

2. **Add Localization Keys**:
- `region.reg_archipelago.name` (EN: `"Azure Archipelago"`, AR: `"أرخبيل اللازورد"`)

3. **Bridge to Existing Regions**:
To maintain graph connectivity, at least one territory in `reg_archipelago` must connect to a frontier territory of an existing adjacent region (e.g. `ter_cst_005`).

---

## 4. Verification Checklist

Before committing new world map data, run the automated test suite to ensure graph integrity:

1. **Run EditMode Tests**:
   - `TerritoryGraphTests`: Verifies that `TerritoryGraph.ValidateSymmetry()` and `IsGraphConnected()` return true.
   - `WorldMapDataTests`: Verifies that all territories reference valid regions, yields are non-negative, and localization keys exist.
2. **Visual Verification**:
   - Open `Assets/KINGWorldWar/Scenes/MainMenu.unity`.
   - Press Play and verify that `WorldMapView` spawns the new territory tile with its world-space banner.
   - Click/tap the tile to verify that camera smoothly focuses on it and the `TerritoryDetailDrawer` slides in with accurate yields and frontier connections.
