# MILESTONE REPORT – KING WORLD WAR

**Phase:** 07 (Army & Training: Server-Authoritative Design & 5 MVP Archetypes)  
**Status:** PASS  
**Timestamp:** 2026-11-20  
**Active Branch:** `phase-07/army-training`  
**Engine:** Unity 6 (`6000.6.9f1`), Universal Render Pipeline (URP 17.6.0)

---

### 1. Summary

Phase 7 establishes the server-authoritative military recruitment, troop roster management, and technology upgrade architecture for KING WORLD WAR. The system implements the 5 canonical MVP unit archetypes spanning Ground and Air operational categories, strictly adhering to the $0 third-party asset mandate and Untrusted Client principles.

Every military statistic is formally classified as either `ServerAuthoritative` (combat and progression rules) or `DisplayOnly` (UI presentation keys and asset paths). Military forces are tracked via immutable authoritative snapshots (`ArmyRosterSnapshot`), which classify unit numbers across three mutually exclusive operational states: **Available**, **In Training**, and **Deployed**. 

Recruitment operations route through an asynchronous client repository contract (`IArmyRepository`), simulated during development by `LocalMockArmyRepository` with UUIDv4 idempotency caching, queue concurrency limits (hard cap of 2 concurrent operations), and simulated network failure modes. A complete Nakama RPC and PostgreSQL persistence specification has been authored in `docs/api/army-contract.md`.

On the presentation layer, `ArmyScreenView` features real-time category filtering (All, Ground, Air), status count breakdowns, and empty/loading states. An interactive `UnitDetailView` presents military specifications, next-level research perks, and a 3D rotating stage preview. Active training queue items are displayed via `TrainingQueueItemView`, with live server timestamp countdowns and cancellation prompts offering a server-defined 50% resource refund.

Automated verification achieved a **100% pass rate across 247 test cases (185 EditMode, 62 PlayMode)** with 0 compile errors and 0 warnings.

---

### 2. Implemented Features

1. **5 MVP Military Unit Archetypes (`DefaultArmyConfig`):**
   - **Ground Operational Category:**
     * **Main Battle Tank (`unit_tank`):** Heavy frontline armor, high frontal protection, composite plating. Produced in Tank Factory Level 1.
     * **Armored Vehicle (`unit_armored_vehicle`):** 6-wheel mechanized infantry carrier with rapid-fire autocannon. Produced in Tank Factory Level 1.
     * **Heavy Artillery (`unit_artillery`):** Long-range self-propelled siege howitzer delivering area-of-effect saturation bombardment. Produced in Tank Factory Level 2.
   - **Air Operational Category:**
     * **Fighter Aircraft (`unit_fighter_aircraft`):** Supersonic delta-wing interceptor providing air superiority and close air support. Produced in Airfield Level 1.
     * **Recon Aircraft (`unit_recon_aircraft`):** High-aspect strategic glider scout UAV penetrating fog of war. Produced in Airfield Level 1.
   - Naval units strictly postponed to future updates per approved MVP scope.

2. **Stat Authority Classification (`StatAuthority`):**
   - `ServerAuthoritative`: Health, Attack, Defense, Speed, Range, CapacityWeight, CombatPower, Cost, RequiredBuildingType, RequiredBuildingLevel, Tier, NextLevelUpgradeCost, NextLevelUpgradeDurationSeconds.
   - `DisplayOnly`: NameKey, DescKey, RoleKey, IconAssetPath, ModelAssetPath, NextLevelBenefitsKey.

3. **Immutable Authoritative Roster Snapshot (`ArmyRosterSnapshot`):**
   - Tracks standing forces per archetype across `TotalOwned`, `InTraining`, `Deployed`, and calculates `Available = TotalOwned - InTraining - Deployed`.
   - Embeds concurrency revision number and `balanceVersion` string (`1.0.0`).

4. **Reusable Battle Deployment Selection Model (`UnitSelection`):**
   - Pure domain force selection model preparing for Phase 9 auto-resolved combat.
   - Validates that requested units exist in data sources and do not exceed available reserves in `ArmyRosterSnapshot`.
   - Produces `BattleDeploymentRequestDto` with target territory ID and unit quantities with zero client-side combat logic.

5. **Client Repository & Development Mock (`LocalMockArmyRepository`):**
   - Implements `IArmyRepository` with async methods: `GetRosterAsync`, `GetTrainingQueueAsync`, `RequestTrainingAsync`, `RequestCancelTrainingAsync`, `RequestUnitUpgradeAsync`.
   - Enforces concurrency limit (max 2 active training slots).
   - UUIDv4 idempotency cache prevents duplicate charges on replayed requests.
   - Reconciles completed training queues based on server UTC timestamps and updates reserves.

6. **Server Contract Specification (`docs/api/army-contract.md`):**
   - Complete Nakama server runtime and PostgreSQL contract specification.
   - Endpoints: `kww.army.get_roster`, `kww.army.train.request`, `kww.army.train.cancel`, `kww.army.upgrade.request`.
   - PostgreSQL persistence tables: `player_units`, `training_queues`, `idempotency_records`.
   - Server-authoritative 50% refund policy on cancellation and offline progress recovery on app restart.

7. **Tactical UI Presentation & 3D Staging:**
   - `ArmyScreenView`: Categorized grid supporting Ground and Air units, category filter tabs, status breakdowns, and empty states.
   - `UnitDetailView`: Full specification drawer, stat authority presentation, next-level research perks, and smooth 3D preview stage rotation.
   - `TrainingQueueItemView`: Granular state badges (`Loading`, `Pending`, `InProgress`, `Completed`, `Failed`, `Cancelled`), live server timestamp countdowns, and cancellation modal with refund notice.
   - `UnitPrefabGenerator`: Automated generation of 5 original, procedural low-poly unit prefabs ($0 cost, 100% original geometry) in `Assets/KINGWorldWar/Art/Prefabs/Units/`.

8. **Bilingual Military Localization:**
   - 100% key parity in Arabic RTL and English across all 5 units, category filters, status labels, queue states, and detail inspection drawers in `DefaultStringTables.cs`.

---

### 3. Automated Test Verification Results

| Suite | Category | Tests Run | Passed | Failed | Pass Rate |
|---|---|---|---|---|---|
| **EditMode** | Core Domain & Calculations | 68 | 68 | 0 | 100% |
| **EditMode** | Base & Building Logic | 26 | 26 | 0 | 100% |
| **EditMode** | Economy & Accrual | 36 | 36 | 0 | 100% |
| **EditMode** | Alliance Permissions & Domain | 22 | 22 | 0 | 100% |
| **EditMode** | Army Archetypes & Stat Classification | 5 | 5 | 0 | 100% |
| **EditMode** | Unit Selection & Reserves Validation | 5 | 5 | 0 | 100% |
| **EditMode** | Training Queue State Machine | 3 | 3 | 0 | 100% |
| **EditMode** | Army Repository & Idempotency | 6 | 6 | 0 | 100% |
| **EditMode** | Army Bilingual Localization Parity | 3 | 3 | 0 | 100% |
| **EditMode** | Army Service & State Transitions | 11 | 11 | 0 | 100% |
| **PlayMode** | Bootstrap & Scene Flow | 6 | 6 | 0 | 100% |
| **PlayMode** | World Map Interaction & Camera | 10 | 10 | 0 | 100% |
| **PlayMode** | Base Building Interaction & Upgrades | 18 | 18 | 0 | 100% |
| **PlayMode** | Economy UI & Real-Time Projections | 8 | 8 | 0 | 100% |
| **PlayMode** | Alliance UI & Permission Matrix | 4 | 4 | 0 | 100% |
| **PlayMode** | 3D Staging Ground & Unit Parade | 12 | 12 | 0 | 100% |
| **PlayMode** | Army Roster, Filter & 3D Preview Stage | 4 | 4 | 0 | 100% |
| **Total** | **All Verification Suites** | **247** | **247** | **0** | **100%** |

---

### 4. Changed & Added Files Summary

#### Core Domain Abstractions (`KingWorldWar.Core`)
- `Assets/KINGWorldWar/Core/Army/UnitCategory.cs` [NEW]
- `Assets/KINGWorldWar/Core/Army/StatAuthority.cs` [NEW]
- `Assets/KINGWorldWar/Core/Army/TrainingQueueState.cs` [NEW]
- `Assets/KINGWorldWar/Core/Army/ArmyRosterSnapshot.cs` [NEW]
- `Assets/KINGWorldWar/Core/Army/UnitSelection.cs` [NEW]
- `Assets/KINGWorldWar/Core/Army/IArmyRepository.cs` [NEW]
- `Assets/KINGWorldWar/Core/Army/UnitType.cs` [MODIFY]
- `Assets/KINGWorldWar/Core/Army/UnitStats.cs` [MODIFY]
- `Assets/KINGWorldWar/Core/Army/UnitDefinition.cs` [MODIFY]
- `Assets/KINGWorldWar/Core/Army/RecruitmentQueueItem.cs` [MODIFY]
- `Assets/KINGWorldWar/Core/Army/IArmyDataSource.cs` [MODIFY]
- `Assets/KINGWorldWar/Core/Army/IArmyService.cs` [MODIFY]
- `Assets/KINGWorldWar/Core/Army/ArmyRoster.cs` [MODIFY]

#### Infrastructure & Persistence (`KingWorldWar.Infrastructure`)
- `Assets/KINGWorldWar/Infrastructure/Army/LocalMockArmyRepository.cs` [NEW]
- `Assets/KINGWorldWar/Infrastructure/Army/ArmyService.cs` [MODIFY]
- `Assets/KINGWorldWar/Infrastructure/Army/DefaultArmyConfig.cs` [MODIFY]
- `Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs` [MODIFY]

#### UI & Presentation Layer (`KingWorldWar.UI`)
- `Assets/KINGWorldWar/UI/Army/TrainingQueueItemView.cs` [NEW]
- `Assets/KINGWorldWar/UI/Army/UnitDetailView.cs` [NEW]
- `Assets/KINGWorldWar/UI/Army/ArmyScreenView.cs` [MODIFY]
- `Assets/KINGWorldWar/UI/Army/RecruitmentDrawer.cs` [MODIFY]

#### 3D Art & Editor Builders (`KingWorldWar.Editor` / Art)
- `Assets/KINGWorldWar/Editor/UnitPrefabGenerator.cs` [NEW]
- `Assets/KINGWorldWar/Art/Prefabs/Units/Unit_Tank_Placeholder.prefab` [NEW/MODIFY]
- `Assets/KINGWorldWar/Art/Prefabs/Units/Unit_ArmoredVehicle_Placeholder.prefab` [NEW]
- `Assets/KINGWorldWar/Art/Prefabs/Units/Unit_Artillery_Placeholder.prefab` [NEW]
- `Assets/KINGWorldWar/Art/Prefabs/Units/Unit_FighterAircraft_Placeholder.prefab` [NEW]
- `Assets/KINGWorldWar/Art/Prefabs/Units/Unit_ReconAircraft_Placeholder.prefab` [NEW]
- `Assets/KINGWorldWar/Editor/ArmyBuilder.cs` [MODIFY]
- `Assets/KINGWorldWar/Features/Army/PlayerArmyView.cs` [MODIFY]

#### Composition Root & Bootstrap (`KingWorldWar.App`)
- `Assets/KINGWorldWar/App/GameBootstrap.cs` [MODIFY]
- `Assets/KINGWorldWar/App/MainMenuScope.cs` [MODIFY]

#### Automated Test Suites (`KingWorldWar.Tests`)
- `Assets/KINGWorldWar/Tests/EditMode/ArmyDefinitionValidationTests.cs` [NEW]
- `Assets/KINGWorldWar/Tests/EditMode/UnitSelectionTests.cs` [NEW]
- `Assets/KINGWorldWar/Tests/EditMode/TrainingQueueStateMachineTests.cs` [NEW]
- `Assets/KINGWorldWar/Tests/EditMode/ArmyRepositoryTests.cs` [NEW]
- `Assets/KINGWorldWar/Tests/PlayMode/ArmyRosterAndTrainingUITests.cs` [NEW]
- `Assets/KINGWorldWar/Tests/EditMode/ArmyDataTests.cs` [MODIFY]
- `Assets/KINGWorldWar/Tests/EditMode/ArmyLocalizationTests.cs` [MODIFY]
- `Assets/KINGWorldWar/Tests/EditMode/ArmyServiceTests.cs` [MODIFY]
- `Assets/KINGWorldWar/Tests/PlayMode/ArmyInteractionTests.cs` [MODIFY]

#### Documentation & Specifications
- `docs/api/army-contract.md` [NEW]
- `docs/ASSET_PROVENANCE.md` [MODIFY]
- `docs/PROJECT_STATE.md` [MODIFY]

---

### 5. Known Limitations & Next Recommended Phase

1. **Known Limitations:**
   - Naval units remain strictly postponed for future content expansions.
   - Real-time 3D tactical combat is out of scope for MVP (asynchronous auto-resolved battles in Phase 9).
2. **Next Recommended Phase:**
   - **Phase 08: Server Runtime & Nakama Backend Deployment:**
     * Setup Nakama Docker compose environment with PostgreSQL.
     * Implement authoritative Go/TypeScript RPCs for economy, base upgrades, army training, and alliance management.
     * Integrate Nakama client SDK in Unity and transition development mock repositories to live server sessions.
