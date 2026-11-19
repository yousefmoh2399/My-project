# BASE BUILDING & INFRASTRUCTURE SPECIFICATION — KING WORLD WAR

**Status:** APPROVED ARCHITECTURE (Phase 05)  
**Security Level:** UNTRUSTED CLIENT (Server-Authoritative via Nakama)  
**Tactical Grid Dimensions:** $20 \times 20$ cells ($4\text{m} \times 4\text{m}$ per cell)  

---

## 1. Tactical Grid Architecture

The player's military base operates on a pure C# 2D spatial grid (`BaseGrid`):
- **Grid Coordinates:** $(0, 0)$ to $(19, 19)$.
- **Cell Size:** $4.0\text{m} \times 4.0\text{m}$ in Unity world space.
- **World Origin Offset:** $(200, 0, 0)$ in `MainMenu.unity` to prevent spatial overlap with World Map theaters $(0..100, 0..80)$.
- **Occupancy Rules:**
  - Each cell may be occupied by at most one building structure.
  - Rectangular footprint: $(X, Y)$ origin with dimensions $\text{Width} \times \text{Height}$.
  - Boundary bounds: $(X \ge 0) \land (X + \text{Width} \le 20) \land (Y \ge 0) \land (Y + \text{Height} \le 20)$.
  - Collision check: Moving a building validates that all target cells are vacant (ignoring the building's own currently occupied cells).

---

## 2. Military Building Archetypes

| Building Type | Footprint | Primary Role | Output Metric | Max Level |
|---|---|---|---|---|
| **Command Center (HQ)** | $4 \times 4$ | Base progression core; unlocks max building tiers | Max Base Building Level (1–10) | 10 |
| **Barracks** | $3 \times 3$ | Infantry recruitment, garrison capacity | Troop Staging Capacity (50–3,600) | 10 |
| **Supply Depot** | $3 \times 3$ | Strategic resource protection and storage | Resource Storage Cap (50K–3M) | 10 |
| **Radar Station** | $2 \times 2$ | Reconnaissance, threat detection | Operational Recon Range (1–10 Regions) | 10 |
| **Vehicle Factory** | $4 \times 3$ | Armor and mechanized unit staging | Armored Vehicle Staging (10–400) | 10 |

---

## 3. Progression & Balance Curves (Levels 1–10)

### 3.1 Command Center (`CommandCenter`)
- Footprint: $4 \times 4$ cells.
- Role: Progression gating for all other base installations.

| Level | Gold Cost | Oil Cost | Iron Cost | Duration (s) | Req. HQ | Max Structure Level |
|---|---|---|---|---|---|---|
| **1** | 0 | 0 | 0 | 0 | — | 1 |
| **2** | 500 | 200 | 100 | 30 | — | 2 |
| **3** | 1,500 | 600 | 300 | 60 | — | 3 |
| **4** | 3,500 | 1,500 | 800 | 120 | — | 4 |
| **5** | 8,000 | 3,500 | 2,000 | 300 | — | 5 |
| **6** | 18,000 | 8,000 | 4,500 | 600 | — | 6 |
| **7** | 40,000 | 18,000 | 10,000 | 1,200 | — | 7 |
| **8** | 90,000 | 40,000 | 25,000 | 2,400 | — | 8 |
| **9** | 200,000 | 90,000 | 55,000 | 4,800 | — | 9 |
| **10** | 450,000 | 200,000 | 120,000 | 9,600 | — | 10 |

---

### 3.2 Barracks (`Barracks`)
- Footprint: $3 \times 3$ cells.
- Role: Infantry capacity and defense garrison.

| Level | Gold Cost | Oil Cost | Iron Cost | Duration (s) | Req. HQ | Troop Capacity |
|---|---|---|---|---|---|---|
| **1** | 200 | 50 | 50 | 15 | 1 | 50 |
| **2** | 600 | 200 | 150 | 45 | 2 | 100 |
| **3** | 1,400 | 500 | 350 | 90 | 3 | 180 |
| **4** | 3,200 | 1,200 | 800 | 180 | 4 | 300 |
| **5** | 7,500 | 3,000 | 1,800 | 360 | 5 | 500 |
| **6** | 16,000 | 6,500 | 4,000 | 720 | 6 | 800 |
| **7** | 35,000 | 15,000 | 9,000 | 1,500 | 7 | 1,200 |
| **8** | 80,000 | 35,000 | 22,000 | 3,000 | 8 | 1,800 |
| **9** | 180,000 | 80,000 | 50,000 | 6,000 | 9 | 2,600 |
| **10** | 400,000 | 180,000 | 110,000 | 12,000 | 10 | 3,600 |

---

### 3.3 Supply Depot (`SupplyDepot`)
- Footprint: $3 \times 3$ cells.
- Role: Storage limit for Gold, Oil, Iron.

| Level | Gold Cost | Oil Cost | Iron Cost | Duration (s) | Req. HQ | Storage Capacity |
|---|---|---|---|---|---|---|
| **1** | 300 | 100 | 100 | 20 | 1 | 50,000 |
| **2** | 800 | 300 | 250 | 50 | 2 | 100,000 |
| **3** | 1,800 | 700 | 550 | 100 | 3 | 180,000 |
| **4** | 4,000 | 1,600 | 1,200 | 200 | 4 | 300,000 |
| **5** | 9,000 | 3,800 | 2,800 | 400 | 5 | 500,000 |
| **6** | 20,000 | 8,500 | 6,000 | 800 | 6 | 800,000 |
| **7** | 45,000 | 19,000 | 14,000 | 1,600 | 7 | 1,200,000 |
| **8** | 100,000 | 42,000 | 30,000 | 3,200 | 8 | 1,700,000 |
| **9** | 220,000 | 95,000 | 68,000 | 6,400 | 9 | 2,300,000 |
| **10** | 500,000 | 210,000 | 150,000 | 12,800 | 10 | 3,000,000 |

---

### 3.4 Radar Station (`RadarStation`)
- Footprint: $2 \times 2$ cells.
- Role: Reconnaissance scanning range across map theaters.

| Level | Gold Cost | Oil Cost | Iron Cost | Duration (s) | Req. HQ | Recon Range |
|---|---|---|---|---|---|---|
| **1** | 250 | 150 | 80 | 20 | 1 | 1 Region |
| **2** | 700 | 400 | 220 | 50 | 2 | 2 Regions |
| **3** | 1,600 | 900 | 500 | 100 | 3 | 3 Regions |
| **4** | 3,600 | 2,100 | 1,100 | 200 | 4 | 4 Regions |
| **5** | 8,000 | 4,600 | 2,500 | 400 | 5 | 5 Regions |
| **6** | 18,000 | 10,000 | 5,500 | 800 | 6 | 6 Regions |
| **7** | 40,000 | 23,000 | 12,500 | 1,600 | 7 | 7 Regions |
| **8** | 90,000 | 50,000 | 28,000 | 3,200 | 8 | 8 Regions |
| **9** | 200,000 | 110,000 | 60,000 | 6,400 | 9 | 9 Regions |
| **10** | 450,000 | 250,000 | 135,000 | 12,800 | 10 | 10 Regions |

---

### 3.5 Vehicle Factory (`VehicleFactory`)
- Footprint: $4 \times 3$ cells.
- Role: Armored unit staging and mechanics bay.

| Level | Gold Cost | Oil Cost | Iron Cost | Duration (s) | Req. HQ | Armor Capacity |
|---|---|---|---|---|---|---|
| **1** | 400 | 200 | 150 | 25 | 1 | 10 Vehicles |
| **2** | 1,000 | 500 | 400 | 60 | 2 | 20 Vehicles |
| **3** | 2,200 | 1,100 | 900 | 120 | 3 | 35 Vehicles |
| **4** | 5,000 | 2,500 | 2,000 | 240 | 4 | 55 Vehicles |
| **5** | 11,000 | 5,500 | 4,500 | 480 | 5 | 80 Vehicles |
| **6** | 25,000 | 12,500 | 10,000 | 960 | 6 | 115 Vehicles |
| **7** | 55,000 | 28,000 | 22,000 | 1,920 | 7 | 160 Vehicles |
| **8** | 120,000 | 60,000 | 50,000 | 3,840 | 8 | 220 Vehicles |
| **9** | 260,000 | 130,000 | 110,000 | 7,680 | 9 | 300 Vehicles |
| **10** | 600,000 | 300,000 | 250,000 | 15,360 | 10 | 400 Vehicles |

---

## 4. Untrusted Client Security Architecture

1. **Non-Authoritative Client Simulation:**
   - Client displays current countdown timers, level indicators, and costs based on cached configurations.
   - Timers rely strictly on UTC timestamps (`DateTime.UtcNow` vs `UpgradeEndTimeUtc`), not `Time.deltaTime`.
2. **Server Migration Path (Phase 8/9):**
   - The interface `IPlayerBaseService` exposes async contracts:
     `Task<Result<BuildingRuntimeState, GameError>> RequestUpgradeAsync(BuildingId id)`
   - In offline/dev mode, `PlayerBaseService` performs local validation.
   - When connecting to Nakama, the implementation switches to `NakamaPlayerBaseService`, which invokes authoritative server RPCs (`rpc_base_upgrade_start`, `rpc_base_upgrade_cancel`).
