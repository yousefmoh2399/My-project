# MILESTONE REPORT – KING WORLD WAR

**Phase:** 05 (Player Base & Building Systems)  
**Status:** PASS  
**Timestamp:** 2026-11-19  

---

### 1. Summary
Phase 5 implements the tactical military base grid, building structures, and upgrade progression systems for KING WORLD WAR. The system features a pure C# 2D spatial grid (`BaseGrid`) measuring $20 \times 20$ cells ($4\text{m} \times 4\text{m}$ cell spacing) with cell occupancy tracking and rectangular footprint collision validation. Five core military structures are defined with 10-level progression curves: Command Center (HQ), Barracks, Supply Depot, Radar Station, and Vehicle Factory. All building operations strictly respect the Untrusted Client security rule: client timers and levels are non-authoritative simulations calculating progress via UTC timestamps, and the domain service exposes asynchronous contracts (`Task<Result<...>>`) designed for drop-in replacement with server-authoritative Nakama RPCs in Phase 8.

In `MainMenu.unity`, switching to the Base tab smoothly transitions the view to the 3D player base at offset $(200, 0, 0)$. Buildings can be selected via mouse click or mobile touch raycasting (with UI pointer rejection), centering the RTS strategy camera and opening the slide-in `BuildingDetailDrawer`. The drawer presents building intelligence, level badges, resource costs (Gold, Oil, Iron), HQ level prerequisite status, an active Upgrade button, a Cancel button with confirmation modal, and real-time countdown timer synchronization both in the UI drawer and on 3D world-space billboards above the buildings. Bilingual localization has 100% key parity in Arabic RTL and English. Automated tests verified 100% pass rate across 131 test cases (91 EditMode, 40 PlayMode).

---

### 2. Implemented Features
1. **Tactical 2D Base Grid (`BaseGrid`):**
   - $20 \times 20$ cell discrete spatial representation ($4.0\text{m} \times 4.0\text{m}$ cell world dimensions).
   - Cell occupancy tracking preventing overlapping footprints.
   - Out-of-bounds boundary validation preventing structures from exceeding grid perimeters.
   - Relocation validation (`TryMove`) ensuring destination cells are vacant while ignoring the building's own currently occupied cells.
2. **Core Domain & Progression Curves (Levels 1–10):**
   - **Command Center (`CommandCenter`):** $4 \times 4$ footprint, controls maximum allowable tier of all base installations.
   - **Barracks (`Barracks`):** $3 \times 3$ footprint, infantry recruitment and base garrison capacity (50 to 3,600 troops).
   - **Supply Depot (`SupplyDepot`):** $3 \times 3$ footprint, storage capacity protection (50,000 to 3,000,000 resources).
   - **Radar Station (`RadarStation`):** $2 \times 2$ footprint, operational reconnaissance range (1 to 10 regions).
   - **Vehicle Factory (`VehicleFactory`):** $4 \times 3$ footprint, armored vehicle assembly and staging (10 to 400 vehicles).
   - Balanced resource costs (Gold, Oil, Iron) and construction times (15s to 15,360s).
3. **Untrusted Client Compliance & Asynchronous Service:**
   - Client values are explicitly tagged `[NonAuthoritative]`.
   - Upgrade timers calculate progress via absolute UTC timestamps (`DateTime.UtcNow` vs `UpgradeEndTimeUtc`), preventing manipulation via speed hacks or per-frame time scaling.
   - `IPlayerBaseService` exposes asynchronous result contracts (`RequestUpgradeAsync`, `CancelUpgradeAsync`, `MoveBuildingAsync`) facilitating drop-in migration to Nakama RPCs in Phase 8.
4. **3D Interactive Base View (`PlayerBaseView` & `BuildingView`):**
   - Spawns 3D buildings from configured prefabs onto designated grid coordinates.
   - Pointer tap raycast selection supporting mouse and touch with `EnhancedTouchSupport` safety guard.
   - Automatic UI click rejection via `EventSystem.current.IsPointerOverGameObject()`.
   - Selection visual ring highlighting footprint.
   - 3D world-space billboard displaying localized building name, level badge, and live upgrade timer countdown.
   - Smooth camera centering via `StrategyCameraRig`.
5. **UI Structure Detail Drawer (`BuildingDetailDrawer`):**
   - Slide-in side drawer displaying building title, level badge, description, and performance perks.
   - Upgrade requirement breakdown showing Gold, Oil, Iron costs, construction time, and HQ prerequisite status (green when satisfied, red when not met).
   - Action buttons: Upgrade button (disabled if HQ requirement not met), Cancel Upgrade button (with confirmation modal dialog), and Close button.
   - Live countdown display with filling progress bar.
6. **Bilingual Military Localization:**
   - 100% key parity in Arabic RTL and English for all building names, descriptions, and drawer UI elements.
   - Real-time language toggle updates drawer text and world-space billboards with contextual Arabic shaping.
   - Logged all new Arabic building terminology into `docs/localization/REVIEW_QUEUE.md` flagged as `Needs Human Review`.
7. **Documentation & Authoring Guides:**
   - Created `docs/base/BASE_BUILDING_SPEC.md` and `docs/base/ADDING_BUILDINGS.md`.

---

### 3. Files Created
- `Assets/KINGWorldWar/Core/Base/BuildingType.cs`
- `Assets/KINGWorldWar/Core/Base/BuildingId.cs`
- `Assets/KINGWorldWar/Core/Base/BuildingCost.cs`
- `Assets/KINGWorldWar/Core/Base/BuildingLevelData.cs`
- `Assets/KINGWorldWar/Core/Base/BuildingDefinition.cs`
- `Assets/KINGWorldWar/Core/Base/BuildingRuntimeState.cs`
- `Assets/KINGWorldWar/Core/Base/BaseGrid.cs`
- `Assets/KINGWorldWar/Core/Base/IPlayerBaseService.cs`
- `Assets/KINGWorldWar/Infrastructure/Base/DefaultBaseConfig.cs`
- `Assets/KINGWorldWar/Infrastructure/Base/BaseConfigSO.cs`
- `Assets/KINGWorldWar/Infrastructure/Base/InMemoryBaseStateRepository.cs`
- `Assets/KINGWorldWar/Infrastructure/Base/PlayerBaseService.cs`
- `Assets/KINGWorldWar/Features/Base/KingWorldWar.Features.Base.asmdef`
- `Assets/KINGWorldWar/Features/Base/BuildingView.cs`
- `Assets/KINGWorldWar/Features/Base/BaseGridView.cs`
- `Assets/KINGWorldWar/Features/Base/PlayerBaseView.cs`
- `Assets/KINGWorldWar/UI/Base/BuildingDetailDrawer.cs`
- `Assets/KINGWorldWar/Editor/BaseBuilder.cs`
- `Assets/KINGWorldWar/Art/Settings/BaseConfig.asset`
- `Assets/KINGWorldWar/Tests/EditMode/BaseGridTests.cs`
- `Assets/KINGWorldWar/Tests/EditMode/BuildingUpgradeTests.cs`
- `Assets/KINGWorldWar/Tests/EditMode/BuildingLocalizationTests.cs`
- `Assets/KINGWorldWar/Tests/PlayMode/PlayerBaseInteractionTests.cs`
- `docs/base/BASE_BUILDING_SPEC.md`
- `docs/base/ADDING_BUILDINGS.md`
- `docs/reports/phase-05-report.md`

---

### 4. Files Modified
- `Assets/KINGWorldWar/App/GameBootstrap.cs` (Registered base services in `ServiceContainer`)
- `Assets/KINGWorldWar/App/KingWorldWar.App.asmdef` (Added asmdef reference to `KingWorldWar.Features.Base`)
- `Assets/KINGWorldWar/App/MainMenuScope.cs` (Wired `PlayerBaseView`, `BuildingDetailDrawer`, and screen tab switching)
- `Assets/KINGWorldWar/Core/Common/GameError.cs` (Added factory methods for building error codes)
- `Assets/KINGWorldWar/Core/WorldMap/TerritoryId.cs` (Hardened struct default empty value handling)
- `Assets/KINGWorldWar/Core/WorldMap/RegionId.cs` (Hardened struct default empty value handling)
- `Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs` (Added 30+ keys for buildings and drawer UI in EN and AR)
- `Assets/KINGWorldWar/Scenes/MainMenu.unity` (Built player base layout, building drawer, and screen transparency)
- `Assets/KINGWorldWar/Tests/EditMode/KingWorldWar.Tests.EditMode.asmdef` (Added reference to `KingWorldWar.Features.Base`)
- `Assets/KINGWorldWar/Tests/PlayMode/KingWorldWar.Tests.PlayMode.asmdef` (Added reference to `KingWorldWar.Features.Base`)
- `Assets/KINGWorldWar/Tests/PlayMode/BootstrapFlowTests.cs` (Added assertions for base service resolution)
- `docs/localization/REVIEW_QUEUE.md` (Logged building terms for native review)
- `docs/PROJECT_STATE.md` (Updated project milestone state and ADR-0005)

---

### 5. Packages Added / Updated
- **None (0 packages added or updated).** System uses existing Unity 6 URP, UGUI, TextMeshPro, and New Input System. Total third-party asset cost: **$0.00**.

---

### 6. Tests Executed
1. **EditMode Suite (`unity command test run --mode editor`):**
   - `BaseGridTests`: 11 tests (Constructor dimensions, invalid dimensions rejection, coordinate bounds, footprint bounds, placement success, overlap collision rejection, out-of-bounds rejection, valid movement, collision movement rejection, same-position movement, removal and occupancy clearing).
   - `BuildingUpgradeTests`: 9 tests (All 5 archetypes defined, 10 levels with positive costs and durations, start upgrade timer, already upgrading rejection, prerequisite not met rejection, prerequisite satisfied success, cancel upgrade, time-elapsed completion, max level rejection).
   - `BuildingLocalizationTests`: 3 tests (Strict key parity across tables, all building definitions localized, all drawer keys resolved).
   - Existing Core, UI, and WorldMap tests: 68 tests.
   - Total EditMode: **91 tests executed (100% pass rate, 0 failed, 0 errors)**.
2. **PlayMode Suite (`unity command test run --mode playmode --async-tests`):**
   - `PlayerBaseInteractionTests`: 9 tests (Spawns all 5 initial buildings, selection updates view and focuses camera, deselect clears view, raycast selects clicked building, empty raycast deselects, drawer displays correct details and costs, upgrade button initiates upgrade, close button hides drawer, language toggle updates localization in real-time).
   - `BootstrapFlowTests`: 1 test (verified registration of all services including `IPlayerBaseService`).
   - Existing PlayMode tests: 30 tests (`WorldMapInteractionTests`, `MainMenuNavigationTests`, `ErrorScreenTests`, `StrategyCameraRigTests`, `UnitySceneLoaderTests`).
   - Total PlayMode: **40 tests executed (100% pass rate, 0 failed, 0 errors)**.
3. **Grand Total:** **131 automated tests passed (100% pass rate, 0 errors, 0 warnings).**

---

### 7. Verification Results & Known Limitations
- **Verification:**
  - Tapping Base tab in MainMenu activates base 3D sector and focuses camera at $(200, 0, 0)$.
  - Tapping World tab smoothly switches back to strategic World Map.
  - Clicking any base building selects it, highlights footprint, and opens `BuildingDetailDrawer`.
  - Upgrading structures starts live countdown timer in both the drawer and 3D world-space billboards.
  - Cancel button prompts confirmation dialog and cleanly halts construction.
  - Real-time language switching updates building names, levels, and drawer text instantly with proper Arabic RTL shaping.
- **Known Limitations:**
  - Building upgrade timers and resource balances are simulated locally on client; server-authoritative wallet and timer verification will be connected to Nakama in Phase 8.
  - Grid placement mode (drag-and-drop building repositioning UI) will be expanded into a dedicated base-editing layout tool in future UI iterations; current relocation is executed via service methods.

---

### 8. Recommended Next Task
**Proceed to Phase 6: Army & Military Units (Recruitment, Formations, Combat Stats).**
- Implement military unit archetypes (Infantry, Tanks, Artillery) in `KingWorldWar.Core.Army`.
- Implement unit recruitment timers linked to Barracks and Vehicle Factory levels.
- Implement troop staging and army roster management.
