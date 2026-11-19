# ARMY & MILITARY UNITS SPECIFICATION — KING WORLD WAR

**Version:** 1.0.0 (Phase 06)  
**Status:** Implemented & Verified  
**Scope:** Core Domain, Archetypes, Combat Stats, Recruitment Simulation, Base Building Integration

---

## 1. OVERVIEW & PHILOSOPHY

Military units are the primary power projection mechanism in *KING WORLD WAR*. Units are recruited in specialized base structures, organized into standing army rosters, and deployed across the strategic world map for territorial garrisons, scouting, and auto-resolved tactical combat.

### Core Architecture Principles:
1. **Zero Monolithic Singletons:** All unit state, definitions, and queue operations reside in cleanly separated interfaces (`IArmyDataSource`, `IArmyStateRepository`, `IArmyService`) injected via the composition root.
2. **Untrusted Client Simulation:** The mobile client maintains local simulated state with UTC timestamp boundaries. All recruitment validation, resource deduction, and inventory awards will be verified and stored server-side via Nakama RPCs in Phase 8/9.
3. **100% Original Military Identity:** Archetypes, visual low-poly representations, and balance curves are fully original ($0 asset store footprint).

---

## 2. UNIT ARCHETYPES & COMBAT STATS

The MVP defines 4 foundational unit archetypes:

| Archetype | Enum ID | Role | Building Prerequisite | Batch Size | Health (HP) | Attack | Defense | Speed (m/s) | Range (m) | Combat Power |
|---|---|---|---|---|---|---|---|---|---|---|
| **Tactical Infantry** | `UnitType.Infantry` | Frontline / Garrison | Barracks Lv 1 | 10 | 120 | 25 | 15 | 6.0 | 15.0 | 40 |
| **Armored Recon** | `UnitType.ReconVehicle` | Scout & Flanking | Barracks Lv 2 | 1 | 350 | 60 | 40 | 12.0 | 25.0 | 110 |
| **Main Battle Tank** | `UnitType.Tank` | Heavy Armored Strike | Vehicle Factory Lv 1 | 1 | 850 | 160 | 120 | 7.0 | 30.0 | 320 |
| **Heavy Artillery** | `UnitType.Artillery` | Siege & Bombardment | Vehicle Factory Lv 2 | 1 | 500 | 280 | 50 | 4.5 | 55.0 | 380 |

### Combat Power Formula:
Combat Power rating is deterministically evaluated using the formula:
$$\text{CombatPower} = (\text{Attack} \times 2) + (\text{Defense} \times 2) + \lfloor\text{Health} / 5\rfloor$$

Total Player Combat Power is the aggregate sum of all mobilized units:
$$\text{TotalCombatPower} = \sum_{u \in \text{Roster}} (\text{CombatPower}_u \times \text{Count}_u)$$

---

## 3. RECRUITMENT ECONOMY & FORMULAS

### Resource Costs & Durations per Single Unit:

| Archetype | Gold Cost | Oil Cost | Iron Cost | Base Duration (s) |
|---|---|---|---|---|
| **Infantry** | 50 | 10 | 20 | 15s |
| **Recon Vehicle** | 150 | 80 | 70 | 30s |
| **Tank** | 350 | 200 | 250 | 60s |
| **Artillery** | 500 | 300 | 400 | 90s |

### Batch Recruitment Scaling:
For batch recruitment requests with quantity $N$:
$$\text{TotalGold} = \text{Gold} \times N$$
$$\text{TotalOil} = \text{Oil} \times N$$
$$\text{TotalIron} = \text{Iron} \times N$$
$$\text{TotalDuration} = \text{RecruitDurationSeconds} \times N$$

- **Infantry:** Recruited in batches of $N \in [5, 100]$ in steps of 5.
- **Vehicles & Artillery:** Recruited in quantities $N \in [1, 20]$ in steps of 1.

---

## 4. BASE BUILDING INTEGRATION & PREREQUISITES

Recruitment is gated by the player's base building infrastructure managed via `IBaseStateRepository`:
- **Infantry:** Requires `BuildingType.Barracks` at Level $\ge 1$.
- **Recon Vehicle:** Requires `BuildingType.Barracks` at Level $\ge 2$.
- **Main Battle Tank:** Requires `BuildingType.VehicleFactory` at Level $\ge 1$.
- **Heavy Artillery:** Requires `BuildingType.VehicleFactory` at Level $\ge 2$.

If prerequisite buildings are missing or beneath the required level, recruitment requests fail with `GameError.UnitPrerequisiteNotMet` (`UNIT_PREREQUISITE_NOT_MET`).

---

## 5. QUEUE SYSTEM & UTC TIMESTAMPS

1. **Concurrent Queues:** The MVP army service supports up to 2 concurrent training queues (`maxConcurrentQueues: 2`). Additional requests return `GameError.RecruitmentQueueFull` (`RECRUITMENT_QUEUE_FULL`).
2. **Deterministic Progress:**
   - Queues track `StartTimeUtc` and `EndTimeUtc`.
   - Local progress is calculated via normalized ratio:
     $$\text{Progress} = \text{Clamp01}\left(\frac{t_{\text{now}} - t_{\text{start}}}{t_{\text{end}} - t_{\text{start}}}\right)$$
   - Avoids frame-dependent timer drift or offline clock cheating.
3. **Queue Cancellation:** Active recruitment can be cancelled at any time via `CancelRecruitmentAsync(queueId)` with confirmation modal.
4. **Completion Check:** Polled deterministically by `ArmyService.CheckCompletedRecruitments(utcNow)`. Completed items are removed from queues and added to the standing roster (`ArmyRoster`).

---

## 6. CLIENT-SERVER CONTRACT (UNTRUSTED CLIENT)

| Component | Client Responsibility | Server Responsibility (Nakama) |
|---|---|---|
| **Recruitment Request** | Optimistic UI preview, parameter validation | Deduct currency in DB transaction, validate prerequisites, emit authoritative queue |
| **Timer Progression** | Local progress bar rendering based on UTC delta | Evaluates elapsed time on server clock; cannot be sped up by local device clock |
| **Queue Cancellation** | UI prompt, confirmation modal | Server refunds resources, terminates queue record |
| **Troop Inventory** | Reads cached roster for UI dashboard | Authoritative database table `player_army_roster` |
| **Combat Stats** | Local tactical display and estimated ratings | Battle auto-resolution engine executes purely on server |
