# KING WORLD WAR

KING WORLD WAR is an original mobile 3D online military strategy game built with Unity 6 (URP) and an authoritative Nakama backend.

---

## 1. Prerequisites

- **Unity Editor:** `Unity 6000.6.9f1` (macOS / Windows / Linux)
- **Render Pipeline:** Universal Render Pipeline (URP 17.6.0)
- **Target Platforms:** Android (min SDK 26 / Android 8.0, 64-bit ARM64) and iOS (iOS 15.0+)
- **System Tooling:**
  - Xcode 26+ (for iOS builds on macOS)
  - Android SDK / NDK Tools & OpenJDK (via Unity Hub -> Add Modules -> Android Build Support)
  - Git & Git CLI

---

## 2. Project Architecture & Folder Map

All project assets and code reside under `Assets/KINGWorldWar/` with strictly enforced Assembly Definitions:

```text
Assets/KINGWorldWar/
  Core/                 [KingWorldWar.Core.asmdef]
                        Pure C# domain entities, math, logging abstractions, results/errors,
                        null network interfaces. Zero engine UI or feature dependencies.
  Infrastructure/       [KingWorldWar.Infrastructure.asmdef]
                        Scene loading service (ISceneLoader), ScriptableObject configuration
                        providers, platform services. Depends on Core.
  Features/             (Base/, World/, Armies/, Battles/)
                        Domain feature modules with independent asmdefs. Depend on Core.
                        Never depend directly on UI implementations.
  UI/                   [KingWorldWar.UI.asmdef]
                        Reusable UI widgets, Error/Maintenance views, RTL text adapters.
                        Depends on Core and uGUI/TextMeshPro.
  App/                  [KingWorldWar.App.asmdef]
                        Composition root (ServiceContainer), GameBootstrap entrypoint,
                        engine log sink. Depends on Core, Infrastructure, and UI.
  Config/               ScriptableObject configuration assets (GameConfig_Dev.asset).
  Scenes/               Bootstrap.unity (Scene 0), MainMenu.unity (Scene 1).
  Tests/
    EditMode/           [KingWorldWar.Tests.EditMode.asmdef]
                        Unit tests for Core utilities, redactors, config validation.
    PlayMode/           [KingWorldWar.Tests.PlayMode.asmdef]
                        Integration and lifecycle tests for Bootstrap and UI screens.
```

---

## 3. How to Run

1. Open the project in **Unity 6000.6.9f1**.
2. Open the entry scene: `Assets/KINGWorldWar/Scenes/Bootstrap.unity` (Build Index 0).
3. Press **Play**.
4. The deterministic bootstrapper initializes logging, validates `GameConfig_Dev.asset`, registers core services in `ServiceContainer`, and asynchronously loads `MainMenu.unity`.

---

## 4. How to Run Automated Tests

### Via Unity CLI (Connected Editor):
```bash
# Run all EditMode tests
unity command test run --mode editor

# Run all PlayMode tests
unity command test run --mode playmode --async-tests
unity command test status
```

### Headless BatchMode (CI / CLI):
```bash
# EditMode tests
/Applications/Unity/Hub/Editor/6000.6.9f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath . -runTests \
  -testPlatform EditMode \
  -testResults Logs/editmode-results.xml \
  -logFile Logs/editmode.log

# PlayMode tests
/Applications/Unity/Hub/Editor/6000.6.9f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath . -runTests \
  -testPlatform PlayMode \
  -testResults Logs/playmode-results.xml \
  -logFile Logs/playmode.log
```

---

## 5. Branching & Commit Conventions

- Work occurs on feature or phase branches: `phase-XX/<short-slug>`.
- Commit format: `<type>: <description>` (`feat:`, `fix:`, `chore:`, `refactor:`, `test:`, `docs:`).
- Never commit generated cache folders (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/`).
- Never commit credentials, private keys, or passwords.
