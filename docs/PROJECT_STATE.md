# PROJECT STATE — KING WORLD WAR

**Last Updated:** Phase 01 (Unity Foundation & Core Architecture)  
**Active Branch:** `phase-01/foundation`  
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
- Executed automated tests: **17 EditMode tests passed (100%), 4 PlayMode tests passed (100%)**.

---

## 2. DECISIONS REGISTER

- **ADR-0001 (Architecture & Foundation):** Established layer separation using Assembly Definitions, Composition Root pattern (`ServiceContainer`), and pure C# Core domain models.
- **Render Pipeline:** Retained URP 17.6.0 with Mobile quality profile and FSR upscaling.
- **Identity & Package ID:** `KING WORLD WAR`, bundle identifier `com.kingworldwar.game`.
- **Client Trust:** Zero authoritative logic on the client; currency and battle resolution reserved for Nakama server.

---

## 3. OPEN QUESTIONS & OWNER ACTIONS

1. **Mobile Build Support Modules:**
   - Owner needs to install **Android Build Support** (with OpenJDK/NDK) and **iOS Build Support** in Unity Hub for version `6000.6.9f1`.
2. **Tutorial Assets Cleanup:**
   - In a future phase, archive or delete the 154 MB `.webm` tutorial videos in `Assets/Tutorials/Media/` to reduce project size.

---

## 4. HOW TO RUN

1. Open `Assets/KINGWorldWar/Scenes/Bootstrap.unity`.
2. Enter Play Mode.
3. Observe successful transition to `MainMenu.unity`.
