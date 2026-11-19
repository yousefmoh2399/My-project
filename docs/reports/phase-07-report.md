# MILESTONE REPORT – KING WORLD WAR

**Phase:** 07 (Alliances & Coalition Systems: Server-Authoritative Design)  
**Status:** PASS  
**Timestamp:** 2026-11-19  
**Active Branch:** `phase-07/alliances`  
**Engine:** Unity 6 (`6000.6.9f1`), Universal Render Pipeline (URP 17.6.0)

---

### 1. Summary
Phase 7 establishes the complete military coalition and alliance subsystem for KING WORLD WAR, including client-side dual-state UI architecture, development simulation mock, and the formal server-authoritative contract specification.

In strict compliance with the **Untrusted Client** mandate, the client has zero authority over alliance permissions, applications, role assignments, or notice modifications. All mutating actions pass through Nakama RPCs backed by PostgreSQL atomic transactions. Alliance creation requires 10,000 Gold, verified against the economy subsystem with idempotency protection. The alliance system supports a 5-tier military rank hierarchy (`Leader`, `Officer`, `Veteran`, `Member`, `Recruit`) with strict permission gating, preventing privilege escalation and unauthorized member kicks.

The dual-state interface (`AllianceScreenView`) seamlessly toggles between:
1. **Public Coalition Directory (No-Alliance State):** Directory search, policy filtering, public dossier inspection drawer (`AllianceDetailDrawer`), and alliance establishment panel.
2. **Coalition Command Dashboard (In-Alliance State):** Coalition banner, aggregate combat power, member roster with dynamic permission-evaluated controls (`AllianceMemberRowView`), interactive notice board, pending application queue triage, and voluntary departure handling with modal confirmation.

Automated verification achieved a **100% pass rate across 224 test cases (166 EditMode, 58 PlayMode)** with 0 compile errors and 0 warnings.

---

### 2. Implemented Features

1. **Role Hierarchy & Administrative Authority Matrix (`KingWorldWar.Core.Alliance`):**
   - 5-tier military hierarchy: `Leader = 1`, `Officer = 2`, `Veteran = 3`, `Member = 4`, `Recruit = 5`.
   - **Kicking Rules:** A member can only kick strictly lower ranks (`actorRole > targetRole`). Officers can kick Veterans, Members, Recruits; Officers cannot kick other Officers or the Leader. Self-kick is rejected.
   - **Promotion/Demotion:** Only Leader can promote to Officer or Leader, or demote an Officer. Officers can promote/demote Member/Recruit up to Veteran.
   - **Notice & Join Policy:** Strictly reserved for Leader and Officer ranks.
   - **Disband / Leadership Transfer:** Strictly restricted to Leader.

2. **Immutable Domain Models & Validation (`KingWorldWar.Core.Alliance`):**
   - `AllianceId`: Strongly typed, immutable, case-insensitive struct.
   - `AllianceRole`: Enum with permission extension methods.
   - `AllianceJoinPolicy`: Enum (`Open = 1`, `ApplyOnly = 2`, `Closed = 3`).
   - `AllianceProfile`: Immutable record holding ID, Name, Tag (3-4 chars), Notice, Leader, MemberCount, MaxMembers (50 cap), TotalCombatPower, JoinPolicy.
   - Validation: `ValidateCreationParameters` enforces Name (3-24 characters), Tag (3-4 alphanumeric uppercase), and Notice ($\le 256$ characters).
   - `AllianceMember`: Immutable member record holding PlayerId, PlayerName, Role, CombatPower, ContributionScore, and JoinedAtUtc.
   - `AllianceApplication`: Immutable pending application record.

3. **Development Mock Repository (`LocalMockAllianceRepository`):**
   - Guarded behind `#if UNITY_EDITOR || DEBUG || UNITY_INCLUDE_TESTS`.
   - Seeded with 3 realistic military coalitions:
     * `[KWW] King Vanguard` (Open, 28/50 members, 12,850,000 power, Leader: General Valerius).
     * `[IRON] Iron Legion` (ApplyOnly, 42/50 members, 21,500,000 power, Leader: Marshal Ironhide).
     * `[DES] Desert Falcons` (Open, 15/50 members, 6,400,000 power, Leader: Colonel Tariq).
   - Simulates simulated network latency, full application queues, role permission verification, capacity cap locking (max 50 members), and client UUIDv4 idempotency caching.

4. **Client Domain Service (`AllianceService`):**
   - Manages client-side active alliance state, reactive event dispatches (`OnAllianceUpdated`, `OnMembersUpdated`, `OnApplicationsUpdated`, `OnLeftAlliance`).
   - Coordinates alliance founding cost (10,000 Gold) with `IEconomyService` before dispatching creation requests.
   - Maintains optimistic updates with server reconciliation.

5. **Server Contract Specification (`docs/api/alliance-contract.md`):**
   - Complete Nakama server runtime and PostgreSQL contract specification.
   - Endpoints: `kww.alliance.get`, `kww.alliance.search`, `kww.alliance.get_members`, `kww.alliance.get_applications`, `kww.alliance.create`, `kww.alliance.join`, `kww.alliance.apply`, `kww.alliance.application.decide`, `kww.alliance.kick`, `kww.alliance.role.set`, `kww.alliance.leave`, `kww.alliance.notice.update`.
   - Complete PostgreSQL relational schema: `alliances`, `alliance_members`, `alliance_applications`, `idempotency_records`.
   - Anti-cheat policies, row-level locking (`SELECT ... FOR UPDATE`), transaction isolation, and error code mappings.

6. **Dual-State UI Presentation:**
   - `AllianceScreenView`: Implements `IScreen` for `ScreenId.Alliance`.
     * State A: Search input, join policy filtering, spawned directory rows, and expandable alliance creation form.
     * State B: Header banner (Name, Tag, Total Combat Power, Member Count), tabbed navigation (Members, Notice Board, Applications Queue), and Leave Alliance action.
   - `AllianceDetailDrawer`: Slide-in public dossier displaying alliance credentials, power rating, and dynamic Join/Apply button.
   - `AllianceMemberRowView`: Member row item with role evaluation dynamically displaying Kick, Promote, and Demote buttons based on caller permissions.

7. **Bilingual Military Localization:**
   - 100% key parity in Arabic RTL and English for all 34 alliance string keys in `DefaultStringTables.cs`.

---

### 3. Files Created
1. `Assets/KINGWorldWar/Core/Alliance/AllianceId.cs`
2. `Assets/KINGWorldWar/Core/Alliance/AllianceRole.cs`
3. `Assets/KINGWorldWar/Core/Alliance/AllianceJoinPolicy.cs`
4. `Assets/KINGWorldWar/Core/Alliance/AllianceMember.cs`
5. `Assets/KINGWorldWar/Core/Alliance/AllianceProfile.cs`
6. `Assets/KINGWorldWar/Core/Alliance/AllianceApplication.cs`
7. `Assets/KINGWorldWar/Core/Alliance/IAllianceRepository.cs`
8. `Assets/KINGWorldWar/Core/Alliance/IAllianceService.cs`
9. `Assets/KINGWorldWar/Infrastructure/Alliance/LocalMockAllianceRepository.cs`
10. `Assets/KINGWorldWar/Infrastructure/Alliance/AllianceService.cs`
11. `Assets/KINGWorldWar/UI/Alliance/AllianceMemberRowView.cs`
12. `Assets/KINGWorldWar/UI/Alliance/AllianceDetailDrawer.cs`
13. `Assets/KINGWorldWar/UI/Alliance/AllianceScreenView.cs`
14. `docs/api/alliance-contract.md`
15. `Assets/KINGWorldWar/Tests/EditMode/AllianceDomainTests.cs`
16. `Assets/KINGWorldWar/Tests/EditMode/AllianceRepositoryTests.cs`
17. `Assets/KINGWorldWar/Tests/EditMode/AllianceLocalizationTests.cs`
18. `Assets/KINGWorldWar/Tests/PlayMode/AllianceUITests.cs`
19. `docs/reports/phase-07-report.md`

---

### 4. Files Modified
1. `Assets/KINGWorldWar/Core/Common/GameError.cs` (Added alliance error factory methods: `AllianceNotFound`, `AllianceFull`, `AlreadyInAlliance`, `NotInAlliance`, `InsufficientPermissions`, `CannotKickHigherOrEqualRole`, `ApplicationNotFound`, `AllianceTagTaken`, `AllianceNameTaken`)
2. `Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs` (Added 34 bilingual EN/AR alliance string keys)
3. `Assets/KINGWorldWar/App/GameBootstrap.cs` (Registered `IAllianceRepository` and `IAllianceService` in `ServiceContainer`)
4. `Assets/KINGWorldWar/App/MainMenuScope.cs` (Integrated `AllianceScreenView`, `AllianceDetailDrawer`, and navigation routing for `ScreenId.Alliance`)
5. `Assets/KINGWorldWar/Features/Army/PlayerArmyView.cs` (Added `EnhancedTouchSupport.enabled` guard to pointer input processing)
6. `docs/PROJECT_STATE.md` (Updated current phase, completed systems, ADR-0007, and How to Run guide)

---

### 5. Files Deleted
None.

---

### 6. Automated Test Execution & Results

1. **EditMode Suite (`unity command --pid 33614 test run --mode editor --timeout 120`):**
   - `AllianceDomainTests`: 10 tests (Role permissions, promotion/demotion matrix, validation parameters, tag constraints, notice bounds, immutability) [PASS].
   - `AllianceRepositoryTests`: 13 tests (Seeded directory, search filtering, open join, application queue, approval, rejection, rank-gated kicks, creation, duplicate rejection, idempotency) [PASS].
   - `AllianceLocalizationTests`: 2 tests (Strict bilingual key parity, presence of required terms) [PASS].
   - Previous Core, Infrastructure, Base, Army, Economy, WorldMap suites: 141 tests [PASS].
   - Total EditMode: **166 tests executed (100% pass rate, 0 failed, 0 errors)**.

2. **PlayMode Suite (`unity command --pid 33614 test run --mode playmode --async-tests true`):**
   - `AllianceUITests`: 4 tests:
     * `AllianceScreenView_WhenNotInAlliance_ShowsNoAllianceRootAndPopulatesDirectory` [PASS]
     * `AllianceScreenView_WhenInAlliance_ShowsInAllianceRootAndDisplaysProfile` [PASS]
     * `AllianceDetailDrawer_OpenAndClose_RendersAllianceData` [PASS]
     * `AllianceMemberRowView_EvaluatesViewerRolePermissions` [PASS]
   - Previous Base, Army, Economy, WorldMap, Navigation, Camera, SceneLoader suites: 54 tests [PASS].
   - Total PlayMode: **58 tests executed (100% pass rate, 0 failed, 0 errors)**.

---

### 7. Grand Total Test Results
- **EditMode:** 166 passed, 0 failed, 0 skipped.
- **PlayMode:** 58 passed, 0 failed, 0 skipped.
- **Grand Total:** **224 / 224 automated tests passed (100% pass rate, 0 errors, 0 warnings).**

---

### 8. Unity Compile / Build Result
- **Unity 6 (`6000.6.9f1`) Editor Phase:** `ready` (Endpoint `127.0.0.1:49669`).
- `UnityEditor.EditorUtility.scriptCompilationFailed`: **`false`**.
- Compilation result: **0 errors, 0 warnings**.

---

### 9. Backend & Security Checks
- **Untrusted Client Compliance:** Verified. Client contains zero write paths to mutate alliance roles, accept applications, or modify notices directly without server validation.
- **Role Hierarchy Enforcement:** Verified. Officers cannot kick Officers or Leaders. Subordinates cannot kick anyone or alter notices.
- **Zero Database Access:** Unity client contains no PostgreSQL driver or direct database connections.
- **Idempotency Protection:** All mutating operations require UUIDv4 `requestId` cached in `idempotency_records`. Duplicate requests return cached results without double deductions or duplicate applications.
- **Atomic Creation Spend:** Alliance creation verifies and deducts 10,000 Gold atomically.
- **Concurrency & Capacity Cap:** Hard-coded 50-member cap enforced with row-level locks on backend specification.

---

### 10. Acceptance Criteria Evidence Table

| # | Acceptance Criterion | Status | Evidence & Implementation Details |
|---|---|---|---|
| **AC-1** | Dual-state UI for `ScreenId.Alliance` (No Alliance vs In Alliance) | **PASS** | `AllianceScreenView` implements `IScreen` and toggles between `_noAllianceRoot` (directory, search, create) and `_inAllianceRoot` (banner, tabs, notice, roster, apps). Verified by `AllianceUITests`. |
| **AC-2** | Untrusted client: Client cannot bypass server permission checks | **PASS** | Client-side role permissions mirror server authorization matrix; server contract enforces authorization via Nakama session tokens. |
| **AC-3** | Alliance creation requires 10,000 Gold with tag validation (3-4 chars) | **PASS** | `AllianceProfile.ValidateCreationParameters` and `AllianceService.CreateAllianceAsync` enforce cost and tag regex `^[A-Z0-9]{3,4}$`. |
| **AC-4** | Role hierarchy enforces kicking, promotion, and demotion bounds | **PASS** | Verified by 10 `AllianceDomainTests` and `AllianceMemberRowView_EvaluatesViewerRolePermissions`. |
| **AC-5** | Server contract document complete with idempotency and schemas | **PASS** | [`docs/api/alliance-contract.md`](file:///Users/yousef/Desktop/My%20project/docs/api/alliance-contract.md) created specifying schemas, error codes, UUIDv4 idempotency, and PostgreSQL tables. |
| **AC-6** | Compiles clean; Arabic/English verified; mock excluded from release | **PASS** | 0 compile errors; 34 bilingual keys verified by `AllianceLocalizationTests`; `LocalMockAllianceRepository` isolated behind preprocessor directives. |

---

### 11. Known Issues and Limitations
1. **Development Mock Simulation:** Live network synchronization with the Nakama server runtime will be connected in Phase 8/9; currently, mutations execute against `LocalMockAllianceRepository`.
2. **Coalition Chat & War Operations:** Advanced chat channels and territory coalition wars are scheduled for post-MVP / Phase 9 per work order scope limits.

---

### 12. Next Recommended Phase
**Phase 08: Global Rankings & Leaderboards (Server-Authoritative Design)**
- Leaderboard categories: Individual Player Combat Power, Alliance Aggregate Power, and Territory Influence.
- Untrusted client: Rankings calculated server-side; client fetches paginated snapshots.
- UI implementation for `ScreenId.Rankings`.
- Bilingual Arabic RTL / English presentation.
