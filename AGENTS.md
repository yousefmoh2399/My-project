# AGENTS.md — RULES FOR AI CODING AGENTS IN KING WORLD WAR

This document sets mandatory guidelines, safety protocols, and architecture rules for any autonomous agent working on KING WORLD WAR.

---

## 1. NON-NEGOTIABLE SAFETY RULES

1. **Inspect Before Changing:** Inspect the existing project before making any changes.
2. **Never Delete/Overwrite Without Approval:** Never delete, overwrite, rename, or migrate existing files without explaining the impact and receiving explicit approval.
3. **Zero Secrets in Code:** Never commit secrets, passwords, private keys, access tokens, or production credentials. Use environment variables and local git-ignored files; commit only `*.example` templates.
4. **Untrusted Client:** The Unity client is untrusted. Currency, inventory, timers, ownership, battle outcomes, rewards, alliance permissions, and ranking inputs must be validated and decided by the server.
5. **No Direct DB Access:** Unity NEVER connects to PostgreSQL directly. Unity interacts with Nakama server-side RPCs and APIs exclusively.
6. **Server-Authoritative Sensitive Operations:** Use server-authoritative Nakama RPCs or documented server-side mechanisms for all mutations.
7. **Idempotency & Transactions:** Use transactions and idempotency keys wherever duplicate requests could grant rewards or spend resources twice.
8. **Platform Compatibility:** Keep systems modular, testable, documented, and fully compatible with Android and iOS. No editor-only APIs in runtime code.
9. **Asset & Package Approval Gate:** Do not add packages or paid assets without stating: why, license, cost, version, and alternatives. Wait for owner approval.
10. **Original Identity:** Do not use Warpath code, branding, maps, models, UI, audio, or any copyrighted asset. Maintain original identity.
11. **Verification Before Claiming:** Do not claim a feature is complete unless it has been built and tested with verified output.
12. **Resolve Ambiguity:** If a requirement is ambiguous, stop and list the exact questions. Do not invent product decisions.
13. **Pre-Edit Summaries:** Before edits, summarize the plan and affected files. For high-impact changes, wait for approval.
14. **Automated Verification:** After edits, run compilation checks and automated tests where possible.
15. **Transparent Reporting:** Report changed files, tests run, results, known limitations, and the next recommended task.

---

## 2. ASSEMBLY DEFINITION & FOLDER CONVENTIONS

```text
               ┌───────────────────────────┐
               │      KingWorldWar.App     │  (Composition Root)
               └─────────────┬─────────────┘
                             │
     ┌───────────────────────┼───────────────────────┐
     ▼                       ▼                       ▼
┌───────────────────────────┐ ┌────────────────┐ ┌──────────────────────────┐
│KingWorldWar.Infrastructure│ │KingWorldWar.UI │ │  KingWorldWar.Features.* │
└─────────────┬─────────────┘ └───────┬────────┘ └────────────┬─────────────┘
              │                       │                       │
              └───────────────────────┼───────────────────────┘
                                      ▼
                        ┌───────────────────────────┐
                        │      KingWorldWar.Core    │  (Pure C# Abstractions)
                        └───────────────────────────┘
```

- **`KingWorldWar.Core`**:
  - Must remain pure C# abstractions, domain models, interfaces, and utilities.
  - No dependencies on other project assemblies.
  - Zero dependencies on `UnityEngine.UI` or `TextMeshPro`.
- **`KingWorldWar.Infrastructure`**:
  - Implements interfaces defined in Core (e.g. `ISceneLoader`, `GameConfig`).
  - Depends only on `KingWorldWar.Core`.
- **`KingWorldWar.UI`**:
  - Monobehaviours, UI widgets, views, and presenters.
  - Depends on `KingWorldWar.Core`, `UnityEngine.UI`, and `Unity.TextMeshPro`.
  - Must NOT be referenced by domain features.
- **`KingWorldWar.Features.*`**:
  - Encapsulated gameplay feature modules.
  - Depend only on `KingWorldWar.Core`. Never depend on UI implementations.
- **`KingWorldWar.App`**:
  - Composition root and entrypoint (`GameBootstrap`, `ServiceContainer`).
  - Wires dependencies together.

---

## 3. HOW TO ADD A FEATURE

1. **Plan & Gate:** Propose the feature scope, affected files (`[NEW]`, `[MODIFY]`, `[DELETE]`), and test plan. Wait for approval if modifying existing files.
2. **Define Abstractions in `Core`:** Declare data models, contracts, and error codes in `Assets/KINGWorldWar/Core/`.
3. **Implement Feature Logic:** Create `Assets/KINGWorldWar/Features/<FeatureName>/` with its own `.asmdef` referencing `KingWorldWar.Core`.
4. **Register in Composition Root:** Wire dependencies through `ServiceContainer` in `GameBootstrap.cs` or local scene scopes.
5. **Add Tests:**
   - Pure domain logic -> `Assets/KINGWorldWar/Tests/EditMode/<FeatureName>Tests.cs`
   - Monobehaviour / lifecycle logic -> `Assets/KINGWorldWar/Tests/PlayMode/<FeatureName>Tests.cs`
6. **Verify Compilation & Tests:** Run automated test runner via CLI and confirm 0 errors, 0 warnings, 100% pass rate.
7. **Commit & Report:** Produce clean commit on phase branch and output milestone report.

---

## 4. FORBIDDEN ACTIONS FOR AGENTS

- Never modify files outside approved phase scope.
- Never commit to `main` directly.
- Never force-push or rewrite git history.
- Never put passwords or tokens in ScriptableObjects or source code.
- Never connect Unity directly to database ports (PostgreSQL 5432).
- Never use monolithic `GameManager` singletons with public mutable state.
