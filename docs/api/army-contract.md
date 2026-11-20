# ARMY & TRAINING SERVER CONTRACT SPECIFICATION

**Protocol Version:** 1.0.0 (Phase 07)  
**Endpoint Target:** Nakama Server Runtime (Go / TypeScript RPCs)  
**Persistence:** PostgreSQL (`player_units`, `training_queues`, `idempotency_records`)  
**Client Status:** Untrusted client; presentation, local countdown projection, and deployment requests only

---

## 1. ARCHITECTURAL & SECURITY FOUNDATION

1. **Untrusted Client:** The Unity client is untrusted. All military unit quantities, active queues, completion validations, resource debits, and unit stat definitions are computed and authorized exclusively by the server. The client possesses **zero write paths** to alter army numbers or complete training prematurely.
2. **No Direct Database Access:** The Unity client NEVER connects to PostgreSQL directly (port 5432 is blocked). All operations pass through authenticated Nakama RPCs.
3. **Server-Authoritative Stats & Balance Versioning:**
   - Every stat used in combat or progression (`Health`, `Attack`, `Defense`, `Speed`, `Range`, `CapacityWeight`, `CombatPower`, `Cost`, `Duration`) is **ServerAuthoritative**.
   - Client definitions serve solely as a local cache for UI display and prediction.
   - Every roster snapshot includes a `balance_version` string (e.g. `"1.0.0"`). If the server balance version increments, clients reconcile during the next sync.
4. **Queue Capacity Enforcement:**
   - Players possess a hard limit of **2 concurrent training operations**.
   - If an incoming `kww.army.train.request` detects $\ge 2$ active or pending queues, the transaction immediately rejects with `RECRUITMENT_QUEUE_FULL`.
5. **Idempotency & Replay Protection:**
   - Every mutating request (`train.request`, `train.cancel`, `upgrade.request`) requires a client-generated UUIDv4 `request_id`.
   - The server verifies `idempotency_records` within an atomic transaction. Replays return the cached response with zero duplicate resource charges.
6. **Cancellation & Refund Policy:**
   - When a player cancels an ongoing training operation, the server cancels the queue and credits a server-defined **50% resource refund** (Gold, Oil, Iron) to the player's wallet within the same atomic transaction.
   - If the queue has already completed server-side (due to timestamp passage or clock skew), cancellation is rejected with `INVALID_PAYLOAD` ("Operation already completed"), and units are added to standing forces.
7. **Offline Progression & Crash Recovery:**
   - All queue timers are bound to server UTC timestamps (`start_time_utc`, `end_time_utc`).
   - When a client restarts or reconnects after disconnection, `kww.army.get_roster` reconciles all completed queues that matured while offline, automatically crediting units to `player_units` and archiving finished queue records.

---

## 2. DATABASE PERSISTENCE SCHEMAS (POSTGRESQL)

```sql
-- 1. Player Standing Forces & Reserves
CREATE TABLE IF NOT EXISTS player_units (
    player_id VARCHAR(64) NOT NULL,
    unit_type INT NOT NULL, -- 1=Tank, 2=ArmoredVehicle, 3=Artillery, 4=FighterAircraft, 5=ReconAircraft
    total_owned INT NOT NULL DEFAULT 0 CHECK (total_owned >= 0),
    in_training INT NOT NULL DEFAULT 0 CHECK (in_training >= 0),
    deployed INT NOT NULL DEFAULT 0 CHECK (deployed >= 0),
    level INT NOT NULL DEFAULT 1 CHECK (level >= 1),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (player_id, unit_type)
);

-- 2. Authoritative Military Training Queues
CREATE TABLE IF NOT EXISTS training_queues (
    queue_id VARCHAR(64) PRIMARY KEY,
    player_id VARCHAR(64) NOT NULL,
    unit_type INT NOT NULL,
    count INT NOT NULL CHECK (count > 0),
    start_time_utc TIMESTAMPTZ NOT NULL,
    end_time_utc TIMESTAMPTZ NOT NULL,
    gold_spent BIGINT NOT NULL DEFAULT 0,
    oil_spent BIGINT NOT NULL DEFAULT 0,
    iron_spent BIGINT NOT NULL DEFAULT 0,
    state INT NOT NULL DEFAULT 2, -- 1=Pending, 2=InProgress, 3=Completed, 4=Failed, 5=Cancelled
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_training_queues_active 
ON training_queues(player_id, state) 
WHERE state IN (1, 2);

-- 3. Idempotency Records
CREATE TABLE IF NOT EXISTS idempotency_records (
    player_id VARCHAR(64) NOT NULL,
    request_id UUID NOT NULL,
    endpoint VARCHAR(64) NOT NULL,
    response_payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (player_id, request_id)
);
```

---

## 3. API ENDPOINTS & SCHEMAS

### 3.1 `kww.army.get_roster`
Fetches the player's full standing roster, unit counts across statuses (`Available`, `InTraining`, `Deployed`), and active training operations.

- **RPC Method:** `kww.army.get_roster`
- **Authentication:** Bearer Session Token
- **Mapping:** POST `/v2/rpc/kww.army.get_roster`

#### Request Payload:
```json
{}
```

#### Response Payload (`200 OK`):
```json
{
  "player_id": "usr_98a7f21b",
  "revision": 142,
  "balance_version": "1.0.0",
  "server_time_utc": "2026-11-20T10:00:00.000Z",
  "roster": [
    {
      "unit_type": 1,
      "category": "Ground",
      "total_owned": 25,
      "in_training": 5,
      "deployed": 10,
      "available": 10,
      "level": 1
    },
    {
      "unit_type": 2,
      "category": "Ground",
      "total_owned": 40,
      "in_training": 0,
      "deployed": 15,
      "available": 25,
      "level": 1
    },
    {
      "unit_type": 3,
      "category": "Ground",
      "total_owned": 12,
      "in_training": 0,
      "deployed": 0,
      "available": 12,
      "level": 1
    },
    {
      "unit_type": 4,
      "category": "Air",
      "total_owned": 8,
      "in_training": 2,
      "deployed": 4,
      "available": 2,
      "level": 1
    },
    {
      "unit_type": 5,
      "category": "Air",
      "total_owned": 15,
      "in_training": 0,
      "deployed": 5,
      "available": 10,
      "level": 1
    }
  ],
  "active_queues": [
    {
      "queue_id": "q_f7a29c1d",
      "unit_type": 1,
      "count": 5,
      "start_time_utc": "2026-11-20T09:55:00.000Z",
      "end_time_utc": "2026-11-20T10:05:00.000Z",
      "state": 2,
      "cost": {
        "gold": 1750,
        "oil": 1000,
        "iron": 1250,
        "recruit_duration_seconds": 300
      }
    }
  ]
}
```

---

### 3.2 `kww.army.train.request`
Initiates military unit recruitment in the training queue. Validates base building prerequisites, checks concurrent queue caps, debits resources atomically, and enqueues the training operation.

- **RPC Method:** `kww.army.train.request`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "8d3e2304-4c22-4912-88f1-c8ab1c6198df",
  "unit_type": 1,
  "quantity": 5
}
```

#### Response Payload (`200 OK`):
```json
{
  "queue_item": {
    "queue_id": "q_f7a29c1d",
    "unit_type": 1,
    "count": 5,
    "start_time_utc": "2026-11-20T10:00:00.000Z",
    "end_time_utc": "2026-11-20T10:05:00.000Z",
    "state": 2,
    "cost": {
      "gold": 1750,
      "oil": 1000,
      "iron": 1250,
      "recruit_duration_seconds": 300
    }
  },
  "updated_roster_revision": 143,
  "deducted_resources": {
    "gold": 1750,
    "oil": 1000,
    "iron": 1250
  }
}
```

---

### 3.3 `kww.army.train.cancel`
Cancels an ongoing training queue operation. Frees the queue slot, refunds 50% of the invested resources to the player's wallet, and updates the roster snapshot.

- **RPC Method:** `kww.army.train.cancel`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "e91c4d92-231a-4f51-b01f-01a2c53f88ef",
  "queue_id": "q_f7a29c1d"
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "queue_id": "q_f7a29c1d",
  "refunded_resources": {
    "gold": 875,
    "oil": 500,
    "iron": 625
  },
  "updated_roster_revision": 144
}
```

---

### 3.4 `kww.army.upgrade.request`
Upgrades unit archetype technology/tier. Validates building prerequisites and research costs.

- **RPC Method:** `kww.army.upgrade.request`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "7b1c3e98-1123-4df1-80a2-11bc9922a012",
  "unit_type": 1
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "unit_type": 1,
  "new_level": 2,
  "updated_roster_revision": 145
}
```

---

## 4. BATTLE DEPLOYMENT DTO (MVP AUTO-RESOLUTION INTEROP)

To maintain decoupling, the unit selection system does **not** simulate battles or mutate rosters. It generates a pure DTO for battle deployment:

```json
{
  "player_id": "usr_98a7f21b",
  "target_territory_id": "cst_001",
  "selected_units": [
    { "unit_type": 1, "quantity": 8 },
    { "unit_type": 2, "quantity": 12 },
    { "unit_type": 4, "quantity": 2 }
  ],
  "total_capacity_weight": 56,
  "client_timestamp_utc": "2026-11-20T10:15:00.000Z"
}
```

The server authoritative battle resolver validates that `quantity <= available` for each unit type, marks them as `deployed` in `player_units`, and resolves combat asynchronously.
