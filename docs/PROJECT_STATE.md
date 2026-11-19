# PROJECT STATE — KING WORLD WAR

**Last Updated:** Phase 04 (World Map & Strategic Territories — 5 Regions, 25 Territories)  
**Active Branch:** `phase-04/world-map`  
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
