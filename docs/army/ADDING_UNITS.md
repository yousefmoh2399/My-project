# DEVELOPER GUIDE: ADDING NEW MILITARY UNITS

**Scope:** Adding new military unit archetypes to KING WORLD WAR  
**Relevant Assemblies:** `KingWorldWar.Core`, `KingWorldWar.Infrastructure`, `KingWorldWar.Features.Army`, `KingWorldWar.UI`

---

## 1. OVERVIEW

Adding a new unit archetype requires updating the pure C# domain layer, registering canonical stats and costs, adding bilingual localization strings (Arabic + English), configuring 3D visual representations, and adding automated tests.

Follow the step-by-step procedure below.

---

## 2. STEP-BY-STEP IMPLEMENTATION

### Step 1: Declare the Unit Type
Open `Assets/KINGWorldWar/Core/Army/UnitType.cs` and add the new archetype enum value:
```csharp
public enum UnitType
{
    Infantry = 1,
    Tank = 2,
    Artillery = 3,
    ReconVehicle = 4,
    AttackHelicopter = 5 // [NEW]
}
```

### Step 2: Declare the Unit Identifier
Open `Assets/KINGWorldWar/Infrastructure/Army/DefaultArmyConfig.cs` and define a canonical `UnitId`:
```csharp
public static readonly UnitId HelicopterId = new UnitId("unt_heli");
```

### Step 3: Define Canonical Stats and Costs
In `DefaultArmyConfig.cs`, append the new `UnitDefinition` to `CreateDefaultDefinitions()`:
```csharp
new UnitDefinition(
    id: HelicopterId,
    type: UnitType.AttackHelicopter,
    nameKey: "unit.heli.name",
    descKey: "unit.heli.desc",
    roleKey: "unit.heli.role",
    stats: new UnitStats(
        health: 450,
        attack: 210,
        defense: 70,
        speed: 16.0f,
        range: 35.0f,
        combatPower: 260),
    cost: new UnitCost(
        gold: 400,
        oil: 250,
        iron: 150,
        recruitDurationSeconds: 45),
    requiredBuildingType: BuildingType.VehicleFactory,
    requiredBuildingLevel: 3,
    batchSize: 1)
```

### Step 4: Add Bilingual Localization Strings
Open `Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs` and add exact matching keys to both English and Arabic tables:

```csharp
// In EnglishTable:
{ "unit.heli.name", "Attack Helicopter" },
{ "unit.heli.desc", "Rotary-wing gunship providing close air support and anti-armor missile strikes." },
{ "unit.heli.role", "Air Support / Anti-Armor" },

// In ArabicTable:
{ "unit.heli.name", "مروحية هجومية" },
{ "unit.heli.desc", "مروحية قتالية تقدم الدعم الجوي القريب والضربات الصاروخية المضادة للدروع." },
{ "unit.heli.role", "إسناد جوي / مضاد للدروع" },
```

Add the Arabic terms to `docs/localization/REVIEW_QUEUE.md` flagged as `Needs Human Review`.

### Step 5: Configure Visual Representation & Staging Position
In `Assets/KINGWorldWar/Features/Army/PlayerArmyView.cs`:
1. Add the unit slot offset in `UnitOffsets`:
```csharp
{ UnitType.AttackHelicopter, new Vector3(30f, 0f, 15f) }
```
2. In `CreateFallbackMesh()`, provide procedural geometry (e.g. fuselage cylinder and rotor blades) or assign a dedicated 3D prefab.

### Step 6: Write Automated Tests
1. **EditMode Data Tests (`Assets/KINGWorldWar/Tests/EditMode/ArmyDataTests.cs`):**
   - Verify definitions count increases.
   - Verify stats validation (`d.Validate()`).
   - Verify prerequisite building requirements.
2. **EditMode Localization Tests (`Assets/KINGWorldWar/Tests/EditMode/ArmyLocalizationTests.cs`):**
   - Automatically checks 100% string key parity between English and Arabic.
3. **PlayMode Tests (`Assets/KINGWorldWar/Tests/PlayMode/ArmyInteractionTests.cs`):**
   - Test unit spawning, raycast selection, and drawer opening for the new unit archetype.

---

## 3. CHECKLIST BEFORE SUBMITTING

- [ ] New `UnitType` declared.
- [ ] Canonical `UnitDefinition` added with positive HP, Attack, Defense, Speed, Range.
- [ ] English and Arabic string tables updated with 100% key parity.
- [ ] `docs/localization/REVIEW_QUEUE.md` updated with human review flags.
- [ ] `ArmyDataTests` updated and passing.
- [ ] `ArmyLocalizationTests` passing with 0 missing keys.
- [ ] 0 errors, 0 warnings across all test suites.
