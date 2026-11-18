# PROJECT STATE — KING WORLD WAR

**Last Updated:** Phase 02 (3D Visual Foundation, URP, Camera Rig, Quality Tiers)  
**Active Branch:** `phase-02/visual-foundation`  
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

---

## 2. DECISIONS REGISTER

- **ADR-0001 (Architecture & Foundation):** Established layer separation using Assembly Definitions, Composition Root pattern (`ServiceContainer`), and pure C# Core domain models.
- **Render Pipeline:** Universal Render Pipeline (URP 17.6.0) with dedicated per-tier assets (`URP_Asset_Low`, `URP_Asset_Medium`, `URP_Asset_High`).
- **Visual Identity:** 100% original procedural low-poly military strategy style. Official palette: Charcoal `#1F2421`, Steel Gray `#4A5568`, Muted Military Green `#3E4E3D`, Restrained Gold `#D4AF37`.
- **Camera Controls:** Decoupled math (`CameraMath`) from MonoBehaviour rig (`StrategyCameraRig`), bounded horizontal panning and zoom heights, New Input System integration.
- **Client Trust:** Zero authoritative logic on the client; currency and battle resolution reserved for Nakama server.

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
