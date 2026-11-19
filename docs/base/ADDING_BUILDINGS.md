# GUIDE: ADDING & MODIFYING BASE BUILDINGS

> **Audience:** Gameplay Designers, Content Creators, and Engineers.  
> **Rule:** Adding or tuning buildings must be done **by data only**, without altering the core simulation engine or UI presenters.

---

## 1. Overview

Buildings in KING WORLD WAR are governed by pure C# immutable data definitions (`BuildingDefinition`, `BuildingLevelData`) stored in `DefaultBaseConfig.cs` and serializable into `BaseConfigSO.cs`.

```text
┌───────────────────────┐       ┌──────────────────────┐       ┌───────────────────────┐
│  DefaultBaseConfig    │ ────> │     BaseConfigSO     │ ────> │  PlayerBaseService    │
│  (C# Balance Curves)  │       │ (ScriptableObject)   │       │ (Upgrade & Placement) │
└───────────────────────┘       └──────────────────────┘       └───────────────────────┘
```

---

## 2. Step-by-Step: Adding a New Building Type

### Step 1: Add Enum Member
In `Assets/KINGWorldWar/Core/Base/BuildingType.cs`:
```csharp
public enum BuildingType
{
    CommandCenter = 1,
    Barracks = 2,
    SupplyDepot = 3,
    RadarStation = 4,
    VehicleFactory = 5,
    Airfield = 6 // [NEW]
}
```

### Step 2: Add Localization Keys
In `Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs`:
Add entries in both `EnglishTable` and `ArabicTable`:
```csharp
// English
{ "building.airfield.name", "Airfield" },
{ "building.airfield.desc", "Flight operations and tactical strike aircraft hangar." },
{ "building.airfield.desc_perk", "Aircraft Capacity: {0}" },

// Arabic
{ "building.airfield.name", "المطار الحربي" },
{ "building.airfield.desc", "مرابض وعمليات الطائرات المقاتلة والضربات الجوية التكتيكية." },
{ "building.airfield.desc_perk", "سعة الطائرات: {0}" },
```
*Remember to log the Arabic terms in `docs/localization/REVIEW_QUEUE.md` for human review.*

### Step 3: Define Level Data
In `Assets/KINGWorldWar/Infrastructure/Base/DefaultBaseConfig.cs`:
Add a factory method:
```csharp
public static BuildingDefinition CreateAirfieldDefinition()
{
    var levels = new List<BuildingLevelData>
    {
        new BuildingLevelData(1, new BuildingCost(1000, 500, 300), 60, requiredHQLevel: 3, outputValue: 4, "building.airfield.desc_perk"),
        new BuildingLevelData(2, new BuildingCost(2500, 1200, 800), 180, requiredHQLevel: 4, outputValue: 8, "building.airfield.desc_perk"),
        // ... up to level 10
    };

    return new BuildingDefinition(
        BuildingType.Airfield,
        "building.airfield.name",
        "building.airfield.desc",
        footprintWidth: 4,
        footprintHeight: 4,
        levels);
}
```
Register it in `DefaultBaseConfig.CreateDefaultDefinitions()`.

### Step 4: Assign Prefab
1. In `Assets/KINGWorldWar/Art/Prefabs/Buildings/`, create the 3D model prefab with a `BoxCollider` or composite collider.
2. Link the prefab in the `PlayerBaseView` inspector or via `BaseBuilder.cs`.

### Step 5: Verify Automated Tests
Run EditMode unit tests:
```bash
unity command test run --mode editor --filter Building
```
Confirm:
- Exactly 10 levels defined.
- Strict key parity across EN and AR.
- Footprint bounds and grid collision checks pass.

---

## 3. Footprint Rules

1. Standard grid is $20 \times 20$ cells.
2. Coordinates must satisfy:
   $$0 \le X \le 20 - \text{Width}$$
   $$0 \le Y \le 20 - \text{Height}$$
3. Avoid placing adjacent buildings on the exact same cells to prevent overlap collisions.
