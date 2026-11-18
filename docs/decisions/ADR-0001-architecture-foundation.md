# ADR-0001: Unity Foundation & Core Architecture Layout

## Status
Accepted

## Context
KING WORLD WAR is a mobile 3D online strategy game targeting Android and iOS, backed by an authoritative Nakama server. To ensure maintainability, rapid compilation, zero cyclic dependencies, and strict security isolation, the project required a clean architecture foundation established before gameplay or networking features were added.

## Decisions

### 1. Assembly Definition Hierarchy
We divided the codebase into discrete, feature-oriented Assembly Definitions with a strict one-way dependency flow:
- `KingWorldWar.Core`: Pure C# business logic, domain models, error handling (`Result<T, GameError>`), and interfaces. Zero dependencies on UI frameworks or other project assemblies.
- `KingWorldWar.Infrastructure`: Implements operational services (e.g. `UnitySceneLoader`, `GameConfig` ScriptableObjects). Depends only on `Core`.
- `KingWorldWar.UI`: Contains view components and screens (e.g. `ErrorScreenView`). Depends on `Core` and Unity UI/TMP. Not referenced by gameplay features.
- `KingWorldWar.App`: The composition root. Orchestrates bootstrap, service registration, and engine log sinks. References `Core`, `Infrastructure`, and `UI`.

### 2. Elimination of Monolithic GameManager
We rejected the anti-pattern of a global, static, mutable `GameManager` singleton. Instead:
- A minimalist, thread-safe `ServiceContainer` is registered at runtime by `GameBootstrap.cs`.
- Services implement explicit interfaces (`ILog`, `ISceneLoader`, `IAuthService`, `IPlayerRepository`).
- Domain logic is kept in Plain Old C# Objects (POCOs), keeping Monobehaviours strictly as view/controller bridges.

### 3. Startup & Scene Flow
- `Bootstrap.unity` is designated as Scene 0 in Build Settings.
- `GameBootstrap.cs` deterministically initializes logging, validates configuration, and loads `MainMenu.unity`.
- In case of initialization or configuration failure, execution halts safely and presents `ErrorScreenView`, preventing null-reference crashes.

### 4. Logging & Credential Redaction
- Logging is wrapped by `ILog` / `GameLog` with structured levels (`Trace`, `Debug`, `Info`, `Warning`, `Error`).
- In release builds, `Trace` and `Debug` logs are stripped via preprocessor guards (`#if DEBUG || UNITY_EDITOR`) and level thresholds.
- `LogRedactor` applies regex sanitization to scrub passwords, tokens, API keys, and connection strings from log outputs.

### 5. Client Untrusted / Non-Secret Configuration
- Client configuration is stored in `GameConfig` ScriptableObjects (`Dev`, `Staging`, `Prod`).
- Strictly non-secret data only (endpoints, version, feature flags). Passwords, database connection strings, and private keys are strictly forbidden in client assets.

## Consequences
- Compilation times are isolated to modified assemblies.
- Domain logic is 100% testable via headless EditMode unit tests without entering PlayMode.
- Architectural boundaries prevent UI or view code from contaminating core domain math and battle rules.
