# ECONOMY & RESOURCE SERVER CONTRACT SPECIFICATION

**Protocol Version:** 1.0.0 (Phase 06)  
**Endpoint Target:** Nakama Server Runtime (Go / TypeScript RPCs)  
**Persistence:** PostgreSQL (`player_wallets`, `resource_ledger`, `idempotency_records`)  
**Client Status:** Untrusted client; presentation and continuous display projection only

---

## 1. ARCHITECTURAL & SECURITY FOUNDATION

1. **Untrusted Client:** The Unity client is untrusted. Currency, inventory, timers, ownership, and resource mutations are computed and authorized exclusively by the server. The client possesses **zero write paths** to alter wallet balances.
2. **No Direct Database Access:** The Unity client NEVER connects to PostgreSQL directly (port 5432 is blocked). All mutations pass through authenticated Nakama RPCs.
3. **Continuous Accrual Model (Why No `economy.collect`):**
   Per the owner's approved production model, resource production uses **continuous server-timestamp accrual**. Balances accrue passively based on elapsed server time and production rate per hour:
   $$\text{Accrued} = \left\lfloor \frac{(\text{CurrentServerTimeUtc} - \text{LastUpdatedServerTimeUtc}).\text{TotalSeconds} \times \text{RatePerHour}}{3600} \right\rfloor$$
   accrual stops strictly at `StorageCap`.  
   *Design Decision:* A manual `economy.collect` button is intentionally **excluded** because:
   - It creates unnecessary database write load and claim-spamming exploits.
   - It adds friction to mobile strategy gameplay.
   - When a transaction occurs (e.g. building upgrade or troop training), the server atomically reconciles passive accrual first, then debits the cost.
4. **Idempotency & Double-Spend Protection:**
   Every mutating request must supply a client-generated UUIDv4 `request_id`. The server maintains an `idempotency_records` table with a uniqueness constraint on `(player_id, request_id)`. Replaying an identical `request_id` immediately returns the original cached response without duplicate resource deductions or queue insertions.
5. **Atomicity & Optimistic Concurrency:**
   All debit operations, ledger entries, and gameplay mutations (e.g. queue creation) execute in a **single atomic PostgreSQL transaction** (`BEGIN ... COMMIT`) with optimistic concurrency checks (`snapshot_version`). If `expected_version != current_version`, the transaction aborts with `STALE_VERSION`.
6. **Immutable Audit Ledger:**
   Every resource balance mutation appends an immutable row to `resource_ledger` recording:
   `(id, player_id, resource_type, amount_delta, reason, reference_id, server_time, balance_after)`.

---

## 2. API ENDPOINTS & SCHEMAS

### 2.1 `kww.economy.get_snapshot`
Fetches the player's authoritative resource balances, storage capacities, production rates, and current snapshot revision.

- **RPC Method:** `kww.economy.get_snapshot`
- **Authentication:** Bearer Session Token (Nakama Auth)
- **HTTP/gRPC Mapping:** POST `/v2/rpc/kww.economy.get_snapshot`

#### Request Payload:
```json
{}
```

#### Response Payload (`200 OK`):
```json
{
  "player_id": "usr_98a7f21b",
  "snapshot_version": 42,
  "last_updated_server_time_utc": "2026-11-19T14:00:00.000Z",
  "balances": {
    "gold": 125000,
    "oil": 48000,
    "iron": 89000
  },
  "storage_caps": {
    "gold_cap": 200000,
    "oil_cap": 200000,
    "iron_cap": 200000
  },
  "protected_caps": {
    "gold_protected": 40000,
    "oil_protected": 40000,
    "iron_protected": 40000
  },
  "production_rates_per_hour": {
    "gold_per_hour": 4000,
    "oil_per_hour": 2800,
    "iron_per_hour": 2000
  }
}
```

---

### 2.2 `kww.base.upgrade.request` (Resource Spend Integration)
Initiates a building upgrade, atomically deducting required Gold, Oil, and Iron from the player's wallet while enrolling the upgrade in the construction queue.

- **RPC Method:** `kww.base.upgrade.request`
- **Authentication:** Bearer Session Token (Nakama Auth)
- **HTTP/gRPC Mapping:** POST `/v2/rpc/kww.base.upgrade.request`

#### Request Payload:
```json
{
  "request_id": "c7a8b419-4822-4a0b-9ef1-8451f50a8b9e",
  "building_id": "bld_tank_factory_01",
  "target_level": 2,
  "expected_snapshot_version": 42,
  "cost": {
    "gold": 1500,
    "oil": 800,
    "iron": 400
  }
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "transaction_id": "tx_8b21c49e7a",
  "request_id": "c7a8b419-4822-4a0b-9ef1-8451f50a8b9e",
  "server_time_utc": "2026-11-19T14:05:00.000Z",
  "authoritative_snapshot": {
    "snapshot_version": 43,
    "last_updated_server_time_utc": "2026-11-19T14:05:00.000Z",
    "balances": {
      "gold": 123833,
      "oil": 47433,
      "iron": 88766
    },
    "storage_caps": {
      "gold_cap": 200000,
      "oil_cap": 200000,
      "iron_cap": 200000
    },
    "production_rates_per_hour": {
      "gold_per_hour": 4000,
      "oil_per_hour": 2800,
      "iron_per_hour": 2000
    }
  },
  "upgrade_operation": {
    "operation_id": "op_bld_upg_102938",
    "building_id": "bld_tank_factory_01",
    "target_level": 2,
    "started_at_server": "2026-11-19T14:05:00.000Z",
    "completes_at_server": "2026-11-19T14:07:00.000Z",
    "duration_seconds": 120
  }
}
```

---

## 3. STRUCTURED ERROR CODES

| Error Code | HTTP | Description | Remediation / Client Handling |
|---|---|---|---|
| `INSUFFICIENT_RESOURCES` | 400 | Current balance + accrued production is less than operation cost. | Display shortfall breakdown in UI; disable confirm action. |
| `PREREQUISITE_NOT_MET` | 400 | Command Center or prerequisite building level is insufficient. | Highlight required building level in drawer. |
| `DUPLICATE_REQUEST` | 409 | Request ID has already been executed. | Server returns original cached result (safe idempotency retry). |
| `STALE_VERSION` | 409 | Client `expected_snapshot_version` does not match server revision. | Client silently refreshes snapshot and prompts user to retry. |
| `RATE_LIMITED` | 429 | Exceeded maximum requests per window (e.g. > 5 req/sec). | Backoff client with exponential jitter. |
| `INVALID_PAYLOAD` | 400 | Missing required fields, negative values, or malformed UUID. | Reject locally in development; log warning. |

---

## 4. SERVER-SIDE DATABASE SCHEMA (PostgreSQL)

```sql
-- 1. Player Wallets (Authoritative State)
CREATE TABLE player_wallets (
    player_id VARCHAR(64) PRIMARY KEY,
    gold BIGINT NOT NULL DEFAULT 10000 CHECK (gold >= 0),
    oil BIGINT NOT NULL DEFAULT 5000 CHECK (oil >= 0),
    iron BIGINT NOT NULL DEFAULT 3000 CHECK (iron >= 0),
    snapshot_version BIGINT NOT NULL DEFAULT 1,
    last_accrual_time TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Immutable Ledger Table (Append-Only)
CREATE TABLE resource_ledger (
    ledger_id BIGSERIAL PRIMARY KEY,
    player_id VARCHAR(64) NOT NULL,
    resource_type VARCHAR(16) NOT NULL, -- 'GOLD', 'OIL', 'IRON'
    amount_delta BIGINT NOT NULL,        -- Positive for grant, negative for debit
    reason VARCHAR(64) NOT NULL,         -- 'BUILDING_UPGRADE', 'TROOP_RECRUIT'
    reference_id VARCHAR(64) NOT NULL,   -- e.g. 'bld_tank_factory_01'
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    balance_after BIGINT NOT NULL
);

-- 3. Idempotency Tracking Table
CREATE TABLE idempotency_records (
    player_id VARCHAR(64) NOT NULL,
    request_id VARCHAR(64) NOT NULL,
    response_payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (player_id, request_id)
);
```

---

## 5. PROPOSED CONFIGURATION & DATA-DRIVEN TUNING

### Production Formula
$$\text{Production}_{\text{accrued}} = \min\left(\text{StorageCap} - \text{Balance}_{\text{last}}, \left\lfloor \frac{\Delta t \times \text{RatePerHour}}{3600} \right\rfloor\right)$$

### Tuning Knobs:
1. **Base Production Multipliers:** Scaled per Resource Depot level (Levels 1–10).
2. **Storage Capacity Multipliers:** Scaled per Command Center & Resource Depot levels.
3. **Protected Vault Capacity:** $20\%$ to $25\%$ of total capacity protected from hostile raids.

---

## 6. ANTI-CHEAT & INTEGRITY POLICIES

1. **Zero Client Write Path:** Client contains no RPC or API capable of directly setting balance fields.
2. **Timestamp Verification:** Client timestamps are completely ignored; elapsed production time $\Delta t$ is computed strictly from `NOW() - last_accrual_time` on the PostgreSQL server.
3. **Payload Sanitization:** Costs passed in request payloads are verified against server-side balance tables; if client supplies forged lower costs, the server rejects the request with `INVALID_PAYLOAD` and logs a security event.
4. **Rate Limiting:** Enforce a maximum of 10 requests per 5-second sliding window per player session in Nakama runtime interceptors.
