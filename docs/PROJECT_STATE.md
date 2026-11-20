# PROJECT STATE — KING WORLD WAR

**Last Updated:** Phase 07 (Army & Training: Server-Authoritative Design)  
**Active Branch:** `phase-07/army-training`  
**Engine:** Unity 6 (`6000.6.9f1`)  
**Active Render Pipeline:** Universal Render Pipeline (URP 17.6.0)

---

## 1. COMPLETED PHASES

### Phase 00 — Project & Environment Audit
- Complete inspection of template project, engine settings, and platform prerequisites.
- Identified need for Git hygiene and missing mobile build modules.
- Formulated recommended architecture approved by owner.

### Phase 01 — Unity Foundation & Core Architecture
- Initialized Git repository on `main` and branched to `phase-01/foundation`.
- Configured `.gitignore`, `.gitattributes`, and `.editorconfig`.
- Created modular folder structure and Assembly Definitions under `Assets/KINGWorldWar/`:
  - `KingWorldWar.Core.asmdef` (Pure C# domain abstractions, zero UI dependencies).
  - `KingWorldWar.Infrastructure.asmdef` (Scene loading, ScriptableObject configs).
  - `KingWorldWar.UI.asmdef` (Error / maintenance screens, UI views).
  - `KingWorldWar.App.asmdef` (Composition root, ServiceContainer, GameBootstrap).
  - `KingWorldWar.Tests.EditMode.asmdef` & `KingWorldWar.Tests.PlayMode.asmdef`.
- Built deterministic startup flow:
  - `Assets/KINGWorldWar/Scenes/Bootstrap.unity` (Build index 0).
  - `Assets/KINGWorldWar/Scenes/MainMenu.unity` (Build index 1).
  - `GameBootstrap.cs` entry point validating configuration, initializing logging, and asynchronously loading `MainMenu`.
- Implemented `UnitySceneLoader` with progress callback, cancellation, and error handling.
- Implemented `GameLog` wrapper with automated regex credential redaction (`LogRedactor`).
- Implemented `GameConfig` ScriptableObject with environment separation and safety validation.
- Implemented `ErrorScreenView` for error handling and maintenance mode.
- Created placeholder networking contracts (`IAuthService`, `IPlayerRepository`) with offline null implementations.
- Executed automated tests: **17 EditMode + 4 PlayMode tests (100% pass rate)**.
- Remediation `REV-02` (Appendix B): Added 6 isolated PlayMode unit tests for `UnitySceneLoader` error handling (**27 total tests passed, 100%**).

### Phase 02 — 3D Visual Foundation, Camera Controls & Quality Tiers
- Established official original military-modern palette in `PaletteSO` (Charcoal, Steel Gray, Muted Military Green, Restrained Gold).
- Created pure C# camera mathematical models in `CameraMath` (XZ rectangular boundary clamping, zoom damping, pinch scaling).
- Implemented production RTS `StrategyCameraRig` with desktop mouse and mobile multi-touch gestures, bounded pan/zoom, and UI pointer filtering.
- Implemented `QualityService` with automatic device hardware heuristics (RAM, GPU memory, CPU cores) and persistent `PlayerPrefs` override.
- Created 3 tuned URP assets for Low, Medium, and High quality tiers.
- Built 5 original low-poly building prefabs (HQ, Barracks, Supply Depot, Radar Station, Vehicle Factory) and 2 unit prefabs (Tank, Aircraft).
- Created `Assets/KINGWorldWar/Scenes/VisualTest.unity` (Build index 2) with ground terrain (120x120m), directional sunlight, base layout, camera rig, and on-screen `DebugOverlayView` (FPS, Tier, Resolution, Scale).
- Documented `docs/ASSET_PROVENANCE.md` (100% original, $0 third-party assets) and `docs/QUALITY_TIERS.md`.
- Automated test coverage: **33 EditMode tests + 14 PlayMode tests = 47 total tests (100% pass rate)**.

### Phase 03 — UI Foundation & Localization (Arabic RTL + English)
- **UI Stack:** Native UGUI + TextMeshPro with reference resolution `1080x1920` (Match 0.5) conforming to mobile safe-area (`SafeAreaFitter`).
- **Localization Subsystem:** Lightweight, pure C# `LocalizationService` supporting instant runtime language toggling without application restart.
- **Pure C# Arabic Text Shaper:** `ArabicTextShaper` in `KingWorldWar.Core.Localization` supporting contextual Presentation Forms-B glyph mapping, Lam-Alif ligatures, and bidirectional formatting preserving LTR digits in mixed Arabic runs (e.g. `"المستوى 15"`).
- **Navigation Subsystem:** Stack-based `IScreenNavigationService` and `ScreenNavigationService` with push, pop, replace, history tracking, and Android hardware Back button / Keyboard `Escape` support.
- **Modal Dialog Subsystem:** Queueing modal service `IModalService` and `ModalService` with sequential FIFO presentation, backdrop click dismiss, and raycast blocking (`ModalDialogView`).
- **Reusable Tactical Components:**
  - `SafeAreaFitter`: Anchors canvas views dynamically to `Screen.safeArea`.
  - `KwwButton`: Enforces mobile accessibility minimum touch targets (>= 48x48dp) and WCAG compliant palettes.
  - `LocalizedText`: Auto-subscribes to language changes, reshapes Arabic text, and auto-mirrors text alignment.
  - `ResourceHeaderView`: Top-bar readout for non-authoritative client values of Gold, Oil, and Iron.
- **Main Menu Shell (`Assets/KINGWorldWar/Scenes/MainMenu.unity`):** Built with bottom bar holding 6 placeholder tabs (World, Base, Army, Alliance, Rankings, Profile), Back button, Language toggle button ("عربي" / "EN"), and top resource header.
- **Documentation:** Created `docs/localization/HOWTO_ADD_STRINGS.md`, `docs/localization/REVIEW_QUEUE.md` (flagging all Arabic military terms as "needs human review"), and `docs/ui/UI_GUIDE.md`.
- **Automated Test Coverage:**
  - 53 EditMode tests (including 7 `ArabicTextShaperTests`, 6 `LocalizationServiceTests` with strict key parity, 4 `NavigationStackTests`, 3 `ModalServiceTests`).
  - 21 PlayMode tests (including 7 `MainMenuNavigationTests` and updated `BootstrapFlowTests`).
  - **74 total automated tests (100% pass rate, 0 errors, 0 warnings).**

### Phase 04 — World Map & Strategic Territories (5 Regions, 25 Territories)
- **Data-Driven Strategic World Model:**
  - 5 fictional regions: Northern Tundra (`reg_north`), Coastal Basin (`reg_coast`), Emerald Oasis (`reg_oasis`), Verdant Valley (`reg_valley`), Iron Ridge (`reg_ridge`).
  - 25 gameplay territories (5 per region, IDs `ter_nrd_001`..`005`, `ter_cst_001`..`005`, `ter_oas_001`..`005`, `ter_val_001`..`005`, `ter_rdg_001`..`005`).
  - Clean separation: Immutable definition (`TerritoryData`) vs Dynamic state (`TerritoryRuntimeState`), explicitly marked non-authoritative client cache until server sync in Phase 8/9.
  - Resource yield model (`ResourceYield`) with Gold, Oil, and Iron yields per hour.
  - ScriptableObject storage (`WorldMapConfigSO`) implementing `ITerritoryDataSource`.
- **Pure C# Topology Graph & Routing:**
  - `TerritoryGraph` validates bidirectional symmetry (`ValidateBidirectionalAdjacency`), connectivity of all 25 nodes, and computes shortest path via BFS routing (`FindShortestPath`).
- **3D Interactive World Map:**
  - `TerritoryView`: 3D visual territory node with terrain tinting, selection ring indicator, and world-space info billboard.
  - `WorldMapView`: Dynamic territory spawning, raycast tap selection with mouse and mobile touch (`EnhancedTouchSupport` guarded), UI click rejection (`IsPointerOverGameObject`), and smooth camera targeting via `StrategyCameraRig`.
- **UI Territory Details Drawer:**
  - `TerritoryDetailDrawer`: Slide-in panel showing territory name, region badge, non-authoritative yields and defense power, frontier connections, disabled Attack/Scout buttons ("Coming in Phase 9"), and accessibility colorblind legend (`★`, `◆`, `✖`, `▲`).
- **Bilingual Military Localization:**
  - 100% string key parity in Arabic RTL and English for all 25 territory names, 25 descriptions, 5 region names/lore, and drawer UI actions.
  - Flagged all 30 region and territory Arabic terms in `docs/localization/REVIEW_QUEUE.md` as "Needs Human Review".
- **Documentation & Authoring Guides:**
  - Created `docs/world/ADDING_TERRITORIES.md` and `docs/WORLD_MAP_SPEC.md`.
- **Automated Test Coverage:**
  - **68 EditMode tests** (including 7 new `TerritoryGraphTests` and 8 new `WorldMapDataTests`).
  - **31 PlayMode tests** (including 10 new `WorldMapInteractionTests` verifying raycast selection, camera focus, UI click isolation, and drawer data binding).
  - **99 total automated tests across all suites (100% pass rate, 0 failed, 0 errors).**

### Phase 05 — Player Base & Building Systems (Full Work Order Compliance)
- **Tactical 2D Base Grid (`BaseGrid`):**
  - $20 \times 20$ cell grid with $4.0\text{m} \times 4.0\text{m}$ cell world dimensions.
  - Cell occupancy tracking with rectangular footprint collision validation.
  - Relocation / Move validation ensuring target cells are vacant (ignoring building's own current cells).
- **Core Domain & All 7 MVP Building Archetypes:**
  - All 7 MVP buildings fully configured with balanced 10-level progression curves:
    1. Command Center (`bld_command_center` / `bld_hq`): $4 \times 4$ footprint, controls maximum tier gate.
    2. Tank Factory (`bld_tank_factory` / `bld_factory`): $4 \times 3$ footprint, armored vehicle production.
    3. Airfield (`bld_airfield`): $5 \times 3$ footprint, runway and air support.
    4. Arms Factory (`bld_arms_factory`): $4 \times 3$ footprint, weapons manufacturing.
    5. Research Center (`bld_research_center`): $3 \times 3$ footprint, technology progression.
    6. Resource Depot (`bld_resource_depot` / `bld_depot`): $3 \times 3$ footprint, resource storage protection.
    7. Training Center (`bld_training_center` / `bld_barracks`): $3 \times 3$ footprint, infantry recruitment.
    + Auxiliary Radar Station (`bld_radar_01`): $2 \times 2$ footprint, regional reconnaissance.
  - Pure C# abstractions `IBuildingDataSource`, `IBaseStateRepository`, `IBaseRepository`, `IPlayerBaseService`.
- **Untrusted Client Security & Formal Server Contract:**
  - Client state is explicitly non-authoritative simulation (`[NonAuthoritative]`).
  - Upgrade timers calculate progression using UTC timestamps (`DateTime.UtcNow` vs `UpgradeEndTimeUtc`), not malleable frame time.
  - Server contract specified in `docs/api/base-contract.md` covering `base.get`, `base.upgrade.request`, `base.upgrade.cancel`, `base.upgrade.sync` with UUIDv4 idempotency keys, server-authoritative timestamps, and structured error codes.
  - Isolated `LocalMockBaseRepository` guarded with `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.
- **3D Base Presentation & UI:**
  - `PlayerBaseView`: Spawns all 8 buildings with dedicated procedural geometry and `BoxCollider`s, pointer raycast selection with mouse and touch (`EnhancedTouchSupport` safety), UI click rejection, and camera focus synchronization.
  - `BuildingView`: Selection ring, world-space level banner, and real-time upgrade countdown timer.
  - `BuildingDetailDrawer`: Slide-in drawer showing structure intelligence, level, performance perks, upgrade costs vs current resources, HQ prerequisites, real-time timer countdown, Upgrade button, Cancel button, and visible "MOCK DATA (DEV ONLY)" badge when running mock repository.
- **Bilingual Localization:**
  - 100% string key parity in Arabic RTL and English for all building names, descriptions, perks, drawer UI controls, and mock data badge.
- **Documentation:**
  - Created `docs/base/BASE_BUILDING_SPEC.md`, `docs/base/ADDING_BUILDINGS.md`, and `docs/api/base-contract.md`.
- **Automated Test Coverage:**
  - **113 EditMode tests** (including `BaseGridTests`, `BuildingUpgradeTests` with DAG acyclic test, negative tests, and mock isolation, `BuildingLocalizationTests`).
  - **50 PlayMode tests** (including `PlayerBaseInteractionTests` verifying all 7 MVP buildings + radar, mock badge, real-time Arabic update, and `ArmyInteractionTests`).
  - **163 total automated tests across all suites (100% pass rate, 0 failed, 0 errors).**

### Phase 06 — Resources & Economy (Server-Authoritative Design)
- **Culture-Aware Military Resource Formatter (`ResourceFormatter`):**
  - Standard abbreviation rules: `< 1,000` integer, `1.25K`, `3.4M`, `2.5B`.
  - Floor rounding ensures client displays strictly no more than authoritative server balances.
  - Signed 64-bit (`long`) large-number safety and overflow protection against `long.MaxValue`.
  - Visual alert state thresholds: `Zero` (muted), `Normal` (gold/white), `NearCap` ($\ge 90\%$ amber), `Capped` ($100\%$ red), `Stale` (amber).
  - Time-to-full duration calculations towards storage cap.
- **Immutable Domain & Continuous Accrual Model (`KingWorldWar.Core.Economy`):**
  - Pure C# domain abstractions: `ResourceType` (Gold, Oil, Iron), `ResourceAmount`, `StorageCapacity`, `ResourceYieldRate`.
  - Server-authoritative `EconomySnapshot` with continuous passive accrual projection:
    $$\text{Accrued} = \min\left(\text{StorageCap} - \text{Balance}, \left\lfloor \frac{\Delta t \times \text{RatePerHour}}{3600} \right\rfloor\right)$$
  - Eliminates manual claim button friction and database claim-spamming exploits.
  - Zero client write path: verified via reflection unit tests asserting absence of public setters or mutating methods.
  - Development mock repository `LocalMockEconomyRepository` simulating latency, optimistic concurrency checks (`STALE_VERSION`), idempotency key caching, and insufficient balance rejection.
- **Server Contract Specification (`docs/api/economy-contract.md`):**
  - Nakama RPC specifications for `kww.economy.get_snapshot` and `kww.base.upgrade.request`.
  - Idempotency via client-generated UUIDv4 `request_id` and PostgreSQL append-only ledger `resource_ledger`.
  - Data-driven configuration proposed via `EconomyConfigSO` (10 levels of storage caps and production rates).
- **UI Presentation & Integration:**
  - `ResourceHeaderView` displays live abbreviated balances with threshold coloring and opens economy details on click.
  - `EconomyDetailDrawer` displays hourly production yields, storage limits, and time-to-full estimates.
  - `BuildingDetailDrawer` checks affordability via `_economyService.CanAfford()` and disables Upgrade button with red shortfall text when balance is insufficient.
- **Bilingual Military Localization:**
  - 100% string key parity in Arabic RTL and English for all 18 economy keys.
- **Automated Test Coverage:**
  - **141 EditMode tests** (including 13 `ResourceFormatterTests`, 7 `EconomySnapshotTests`, 6 `EconomyRepositoryTests`, and 1 `EconomyLocalizationTests`).
  - **54 PlayMode tests** (including 4 new `EconomyUITests`).
  - **195 total automated tests across all suites (100% pass rate, 0 failed, 0 errors).**

### Feature Module — Army & Military Units (Recruitment, Formations, Combat Stats)
- **Unit Archetypes & Combat Domain:**
  - 4 foundational military unit archetypes in `KingWorldWar.Core.Army`: Tactical Infantry (`UnitType.Infantry`), Armored Recon (`UnitType.ReconVehicle`), Main Battle Tank (`UnitType.Tank`), and Heavy Artillery (`UnitType.Artillery`).
  - Robust immutable `UnitId` struct and comprehensive `UnitStats` model (Health, Attack, Defense, Speed, Range, Combat Power auto-calculation).
  - Deterministic combat power rating formula: $\text{CombatPower} = (\text{Attack} \times 2) + (\text{Defense} \times 2) + \lfloor\text{Health} / 5\rfloor$.
  - Resource costs (`UnitCost`) in Gold, Oil, Iron, and duration in seconds, with batch scaling (`Multiply`).
  - Base building prerequisite gating integrated with `IBaseStateRepository` (Barracks Lv 1/2, Vehicle Factory Lv 1/2).
- **Recruitment Queue System & Standing Roster:**
  - `RecruitmentQueueItem` tracking `StartTimeUtc` and `EndTimeUtc` for tamper-resistant progress calculation.
  - Concurrent training queues (up to 2 concurrent operations) with deterministic completion resolution.
  - Standing `ArmyRoster` tracking mobilized forces and total aggregate army combat power rating.
  - Pure C# service contracts: `IArmyDataSource`, `IArmyStateRepository`, and `IArmyService`.
- **3D Army View & Parade Ground:**
  - `PlayerArmyView` situated at world position $(400, 0, 0)$ in `MainMenu.unity`.
  - Spawns all 4 unit archetypes with dedicated collider bounds, golden circular selection rings, and real-time world-space countdown billboards.
  - Raycast selection supporting desktop mouse and mobile multi-touch with smooth camera rig target tracking.
- **UI Army Screen & Recruitment Drawer:**
  - Replaces placeholder screen for `ScreenId.Army` with production `ArmyScreenView` displaying total combat power badge, total forces count, active training operations count, and 4 unit cards.
  - Slide-in `RecruitmentDrawer` featuring unit stats, quantity stepper (`-`, `+`, `Max`), cost breakdown, prerequisite badge, Commence Mobilization button, active training progress bar with countdown, and cancel button with modal confirmation.
- **Bilingual Military Localization:**
  - 100% string key parity in Arabic RTL and English for all unit names, descriptions, roles, combat stats, and drawer UI controls.
  - All Phase 6 Arabic terms cataloged in `docs/localization/REVIEW_QUEUE.md` flagged as `Needs Human Review`.
- **Documentation:**
  - Created `docs/army/ARMY_UNITS_SPEC.md` and `docs/army/ADDING_UNITS.md`.
- **Automated Test Coverage:**
  - **110 EditMode tests** (including 8 new `ArmyDataTests`, 8 new `ArmyServiceTests`, and 3 new `ArmyLocalizationTests`).
  - **50 PlayMode tests** (including 10 new `ArmyInteractionTests` and updated `BootstrapFlowTests`).
  - **160 total automated tests across all suites (100% pass rate, 0 failed, 0 errors).**

---

## 2. DECISIONS REGISTER

- **ADR-0001 (Architecture & Foundation):** Established layer separation using Assembly Definitions, Composition Root pattern (`ServiceContainer`), and pure C# Core domain models.
- **Render Pipeline:** Universal Render Pipeline (URP 17.6.0) with dedicated per-tier assets (`URP_Asset_Low`, `URP_Asset_Medium`, `URP_Asset_High`).
- **Visual Identity:** 100% original procedural low-poly military strategy style. Official palette: Charcoal `#1F2421`, Steel Gray `#4A5568`, Muted Military Green `#3E4E3D`, Restrained Gold `#D4AF37`.
- **Camera Controls:** Decoupled math (`CameraMath`) from MonoBehaviour rig (`StrategyCameraRig`), bounded horizontal panning and zoom heights, New Input System integration.
- **Localization & BiDi Strategy:** Custom pure C# string table engine with zero third-party packages ($0 cost). Pure C# contextual Arabic shaping converting to Presentation Forms-B and preserving LTR digit order for TextMeshPro. Strict key parity enforced via automated unit tests.
- **Accessibility & Touch Targets:** All interactive buttons enforce minimum dimensions of 48x48dp per mobile accessibility standards (Apple HIG and Google Material).
- **Client Trust:** Zero authoritative logic on the client; currency and battle resolution reserved for Nakama server.
- **ADR-0004 (World Map Topology & Untrusted Client):** All 25 territories and 5 regions are declared via pure C# domain contracts and immutable ScriptableObject configs. The graph topology enforces 100% bidirectional symmetry and single-component connectivity. All territory runtime values (yields, defense, ownership) are strictly non-authoritative client displays with explicit warnings until server-synchronized in Phase 8/9. Colorblind accessibility symbols (`★`, `◆`, `✖`, `▲`) accompany faction colors.
- **ADR-0005 (Player Base Grid & Upgrade Architecture):** Tactical base operations use a $20 \times 20$ discrete grid with rectangular footprint occupancy. Upgrades enforce Command Center prerequisite tier gating. Client upgrade countdowns rely on UTC timestamps and asynchronous contracts (`Task<Result<...>>`) to cleanly bridge from local in-memory simulation to authoritative Nakama RPCs in Phase 8.
- **ADR-0006 (Army Units & Recruitment Architecture):** Military units use pure C# domain definitions and auto-calculating combat power ratings. Recruitment queues are capped at 2 concurrent operations and rely on UTC timestamps for tamper-resistant client progression. Unit recruitment and standing rosters are explicitly modeled as client-side simulations; authoritative inventory and training verification will be processed via Nakama PostgreSQL transactions in Phase 8/9.
- **ADR-0007 (Alliance Hierarchy, Untrusted Client & Server Contract Architecture):** Alliances enforce a strict server-authoritative role hierarchy (`Leader` > `Officer` > `Veteran` > `Member` > `Recruit`). The client possesses zero write authority: all role updates, applications, kicks, and notice board edits pass through Nakama RPCs and atomic PostgreSQL transactions. Alliance establishment requires 10,000 Gold, tag validation (3-4 alphanumeric), and maximum member cap of 50. All mutating requests enforce UUIDv4 idempotency.

---

### Phase 07 — Army & Training (Server-Authoritative Design & 5 MVP Archetypes)
- **5 MVP Military Unit Archetypes:**
  - **Ground Operational Category:**
    * Main Battle Tank (`unit_tank`): Heavy assault armor, high frontal protection, built in Tank Factory (Level 1).
    * Armored Vehicle (`unit_armored_vehicle`): Mechanized support carrier with rapid-fire autocannon, built in Tank Factory (Level 1).
    * Heavy Artillery (`unit_artillery`): Long-range siege howitzer delivering area bombardment, built in Tank Factory (Level 2).
  - **Air Operational Category:**
    * Fighter Aircraft (`unit_fighter_aircraft`): Supersonic tactical interceptor providing air superiority, built in Airfield (Level 1).
    * Recon Aircraft (`unit_recon_aircraft`): Strategic high-altitude scout UAV unveiling fog of war, built in Airfield (Level 1).
  - Naval units strictly postponed per MVP scope.
- **Stat Classification & Authority Model:**
  - Strict tagging: `ServerAuthoritative` (Health, Attack, Defense, Speed, Range, CapacityWeight, CombatPower, Cost, Requirements, Tier, UpgradeCosts, UpgradeDuration) vs `DisplayOnly` (NameKey, DescKey, RoleKey, IconAssetPath, ModelAssetPath, NextLevelBenefitsKey).
- **Authoritative Roster Snapshot (`ArmyRosterSnapshot`):**
  - Immutable server-authoritative snapshot providing status counts: `TotalOwned`, `InTraining`, `Deployed`, and calculated `Available`.
  - Concurrency revision tracking and balance versioning (`balanceVersion`).
- **Unit Selection Model (`UnitSelection`):**
  - Reusable military force selection for deployment.
  - Strictly validates against available reserves in authoritative snapshot (rejects negative, empty, or over-reserve requests).
  - Pure DTO output (`BattleDeploymentRequestDto`) with zero client-side battle simulation logic.
- **Development Mock Repository (`LocalMockArmyRepository`):**
  - Guarded behind `#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS`.
  - Simulates server-authoritative costs, durations, queue concurrency limits (max 2), UUIDv4 request idempotency caching, latency, and simulated failure scenarios.
- **Server Contract Specification (`docs/api/army-contract.md`):**
  - Protocol Version 1.0.0 for Nakama RPCs and PostgreSQL persistence.
  - Endpoints: `kww.army.get_roster`, `kww.army.train.request`, `kww.army.train.cancel`, `kww.army.upgrade.request`.
  - PostgreSQL schema for `player_units`, `training_queues`, and `idempotency_records`.
  - Server-defined 50% refund policy on cancellation and offline queue progress reconciliation.
- **UI & 3D Staging Ground:**
  - `ArmyScreenView`: Categorized grid supporting Ground and Air units, category filtering tabs (All, Ground, Air), status breakdowns (`Available`, `InTraining`, `Deployed`), and empty/loading states.
  - `UnitDetailView`: Full specification drawer, stat authority presentation, next-level research perks, and smooth 3D preview stage rotation.
  - `TrainingQueueItemView`: Granular state badges (`Loading`, `Pending`, `InProgress`, `Completed`, `Failed`, `Cancelled`), live server timestamp countdowns, and cancellation with refund notice modal.
  - `UnitPrefabGenerator`: Automated generation of 5 original, procedural low-poly unit prefabs ($0 cost, 100% original geometry) in `Assets/KINGWorldWar/Art/Prefabs/Units/`.
- **Bilingual Military Localization:**
  - 100% key parity in Arabic RTL and English for all 5 units, category filters, status labels, queue states, and specification drawers in `DefaultStringTables.cs`.
- **Automated Test Coverage:**
  - **185 EditMode tests** (including 5 new `ArmyDefinitionValidationTests`, 5 new `UnitSelectionTests`, 3 new `TrainingQueueStateMachineTests`, 6 new `ArmyRepositoryTests`, and updated `ArmyLocalizationTests` & `ArmyServiceTests`).
  - **62 PlayMode tests** (including 4 new `ArmyRosterAndTrainingUITests` and updated `ArmyInteractionTests`).
  - **247 total automated tests across all suites (100% pass rate, 0 failed, 0 errors, 0 warnings).**
- **Role Hierarchy & Permission Model (`KingWorldWar.Core.Alliance`):**
  - Roles: `Leader = 1`, `Officer = 2`, `Veteran = 3`, `Member = 4`, `Recruit = 5`.
  - Strict hierarchical permission matrix:
    - Kicking: actor must strictly outrank target (`actorRole > targetRole`); self-kick prohibited via kick command.
    - Role changes: only Leader can promote/demote Officers; Officers can only manage Member/Recruit up to Veteran.
    - Notice & Policy editing: Leader and Officer only.
    - Disband & Leadership transfer: Leader only.
- **Immutable Domain Models:**
  - `AllianceId`: Immutable case-insensitive struct.
  - `AllianceRole`: Enum with permission extension methods (`CanInvite`, `CanManageApplications`, `CanEditNotice`, `CanDisband`, `CanTransferLeadership`, `CanKick`, `CanPromote`, `CanDemote`).
  - `AllianceJoinPolicy`: Enum (`Open = 1`, `ApplyOnly = 2`, `Closed = 3`).
  - `AllianceProfile`: Immutable record with `ValidateCreationParameters` (name 3-24 chars, tag 3-4 alphanumeric, notice <= 256 chars), status properties (`IsFull`, `CanDirectJoin`, `CanApply`).
  - `AllianceMember`: Immutable member record with combat power and contribution score.
  - `AllianceApplication`: Immutable pending application record.
- **Development Mock Repository (`LocalMockAllianceRepository`):**
  - Guarded behind `#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS`.
  - Seeded with 3 realistic coalitions: `[KWW] King Vanguard` (Open, 28/50), `[IRON] Iron Legion` (ApplyOnly, 42/50), and `[DES] Desert Falcons` (Open, 15/50).
  - Simulates server permissions, application queues, member lists, capacity limits (50 cap), and UUIDv4 idempotency caching.
- **Client Service (`AllianceService`):**
  - Manages reactive state updates and events (`OnAllianceUpdated`, `OnMembersUpdated`, `OnApplicationsUpdated`, `OnLeftAlliance`).
  - Coordinates creation spend (10,000 Gold) with `IEconomyService`.
- **Server Contract Specification (`docs/api/alliance-contract.md`):**
  - Complete Nakama server runtime and PostgreSQL contract specification.
  - RPC endpoints: `kww.alliance.get`, `kww.alliance.search`, `kww.alliance.get_members`, `kww.alliance.get_applications`, `kww.alliance.create`, `kww.alliance.join`, `kww.alliance.apply`, `kww.alliance.application.decide`, `kww.alliance.kick`, `kww.alliance.role.set`, `kww.alliance.leave`, `kww.alliance.notice.update`.
  - PostgreSQL schema for `alliances`, `alliance_members`, `alliance_applications`, `idempotency_records`.
  - Anti-cheat, rate limiting, and structured error codes mapping.
- **Dual-State UI Presentation:**
  - `AllianceScreenView`: Implements `IScreen` for `ScreenId.Alliance`.
    - State A (No Alliance): directory search, join policy filtering, and alliance creation panel (10,000 Gold).
    - State B (In Alliance): coalition header banner, Total Combat Power, Member Count / Max, Notice Board tab with live edit capability for Officers/Leaders, Member Roster tab with permission-aware Kick/Promote/Demote buttons, and Applications Queue tab with Accept/Reject triage.
  - `AllianceDetailDrawer`: Slide-in inspection panel displaying alliance dossier, stats, and Join/Apply action button.
  - `AllianceMemberRowView`: Dynamic member row with role evaluation.
- **Bilingual Military Localization:**
  - 100% key parity in Arabic RTL and English for all 34 alliance keys in `DefaultStringTables.cs`.
- **Automated Test Coverage:**
  - **166 EditMode tests** (including 6 new `AllianceDomainTests`, 14 new `AllianceRepositoryTests`, and 2 new `AllianceLocalizationTests`).
  - **58 PlayMode tests** (including 4 new `AllianceUITests`).
  - **224 total automated tests across all suites (100% pass rate, 0 failed, 0 errors).**

---

## 3. OPEN QUESTIONS & OWNER ACTIONS

1. **Mobile Build Support Modules:**
   - Owner needs to install **Android Build Support** (with OpenJDK/NDK) and **iOS Build Support** in Unity Hub for version `6000.6.9f1`.
2. **On-Device Profiling:**
   - Run the on-device profiling checklist documented in `docs/QUALITY_TIERS.md` on physical target devices.
3. **Tutorial Assets Cleanup:**
   - Archive or delete the 154 MB `.webm` tutorial videos in `Assets/Tutorials/Media/` when convenient.

---

## 4. HOW TO RUN

### Running the Alliance Screen & Coalition Operations in Main Menu
1. Open `Assets/KINGWorldWar/Scenes/MainMenu.unity`.
2. Enter Play Mode.
3. In the bottom navigation bar, tap the **Alliance** tab (`ScreenId.Alliance`).
4. **State A (No Alliance):**
   - Observe the Coalition Command directory displaying seeded coalitions: `[KWW] King Vanguard`, `[IRON] Iron Legion`, `[DES] Desert Falcons`.
   - Use the search bar to filter by name or tag.
   - Click on any alliance row to slide open the `AllianceDetailDrawer`, inspecting combat power, member capacity, notice board, and Join/Apply button.
   - Click **Create Alliance** to expand the establishment form (requires 10,000 Gold, 3-4 char tag).
5. **State B (In Alliance):**
   - Join `[KWW] King Vanguard` (Open policy) or found an alliance.
   - Observe the top header banner displaying: Alliance Name, `[TAG]`, Total Combat Power, and Member count ($29/50$).
   - Switch between tabs:
     * **Members Tab:** View all member ranks (Leader, Officer, Veteran, Member, Recruit). Authorized ranks see dynamic Kick, Promote, and Demote action buttons.
     * **Notice Board Tab:** View public bulletin. Officers and Leaders can click "Edit Notice", update text, and save in real-time.
     * **Applications Tab:** Visible to Officers and Leaders. Review pending applications with one-click Accept or Reject buttons.
   - Click **Leave Alliance** to exit with confirmation dialog.
6. Tap the "عربي" / "EN" button in the top navigation bar to observe instant real-time bilingual switching and Arabic RTL shaping across all alliance views and drawers.

### Running the Army Screen & Military Units in Main Menu
1. Open `Assets/KINGWorldWar/Scenes/MainMenu.unity`.
2. Enter Play Mode.
3. In the bottom navigation bar, tap the **Army** tab. The view smoothly transitions to the 3D parade staging ground at offset $(400, 0, 0)$ and displays the full-screen `ArmyScreenView`.
4. Observe the Military Command dashboard displaying: Total Combat Power badge, Total Forces count, Active Training Operations count, and 4 unit cards (Tactical Infantry, Armored Recon, Main Battle Tank, Heavy Artillery).
5. Click **Recruit** on any unit card (or click/tap the 3D unit models directly) to slide open the `RecruitmentDrawer`.
6. Use the quantity stepper (`-`, `+`, `Max`) to adjust batch count; observe immediate real-time cost and duration scaling.
7. Click **Commence Mobilization** to start training; observe the live progress bar and countdown timer in both the drawer and the 3D world-space billboard.
8. Click **Cancel Mobilization** to abort recruitment with a modal confirmation prompt.
9. Tap the "عربي" / "EN" button in the top navigation bar to observe instant real-time bilingual switching and Arabic RTL shaping across all army screens and drawers.

### Running the Player Base in Main Menu
1. Open `Assets/KINGWorldWar/Scenes/MainMenu.unity`.
2. Enter Play Mode.
3. In the bottom navigation bar, tap the **Base** tab. The view smoothly transitions to the 3D player base at offset $(200, 0, 0)$.
4. Click or tap any building (Command Center, Barracks, Supply Depot, Radar Station, Vehicle Factory) to select it; the camera centers on the structure.
5. In the slide-in `BuildingDetailDrawer`, view current level, costs, and perks.
6. Click **Upgrade** to start construction: observe the real-time countdown timer in the drawer and the 3D world-space billboard above the building.
7. Click **Cancel** to abort construction with a confirmation dialog.
8. Tap the **World** tab in the bottom bar to smoothly return to the 3D strategic World Map.

### Running the World Map in Main Menu
1. Open `Assets/KINGWorldWar/Scenes/MainMenu.unity`.
2. Enter Play Mode.
3. Click or tap any 3D territory tile to select it; the camera smoothly pans to center on the selected territory.
4. Observe the slide-in Territory Detail Drawer displaying territory intelligence, resource yields, defense power, frontier connections, and faction accessibility symbols.
5. Tap the "عربي" / "EN" button in the top navigation bar to observe instant real-time bilingual switching and Arabic RTL shaping across all territory details.
6. Click the Close ("X") button or tap another territory to switch selection.

### Running the Visual Test Scene
1. Open `Assets/KINGWorldWar/Scenes/VisualTest.unity`.
2. Enter Play Mode.
3. Use mouse drag (left, right, or middle click) to pan the camera across the tactical base.
4. Use mouse scroll wheel to zoom in and out smoothly.
5. Notice the on-screen debug overlay displaying live FPS, active Quality Tier, screen resolution, and render scale.

### Running the Full Startup Flow
1. Open `Assets/KINGWorldWar/Scenes/Bootstrap.unity`.
2. Enter Play Mode.
3. Observe bootstrap validation and transition to `MainMenu.unity`.
