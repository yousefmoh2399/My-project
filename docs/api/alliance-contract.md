# ALLIANCE & COALITION SERVER CONTRACT SPECIFICATION

**Protocol Version:** 1.0.0 (Phase 07)  
**Endpoint Target:** Nakama Server Runtime (Go / TypeScript RPCs)  
**Persistence:** PostgreSQL (`alliances`, `alliance_members`, `alliance_applications`, `idempotency_records`)  
**Client Status:** Untrusted client; presentation and intent dispatch only

---

## 1. ARCHITECTURAL & SECURITY FOUNDATION

1. **Untrusted Client:** The Unity client is untrusted. Alliance membership, roles, permissions, notices, join policies, and member rosters are validated, executed, and persisted exclusively on the server. The client possesses **zero write paths** to change roles, accept applications, or mutate membership directly.
2. **No Direct Database Access:** The Unity client NEVER connects to PostgreSQL directly (port 5432 is strictly blocked). All operations pass through authenticated Nakama RPCs.
3. **Role Hierarchy & Authority Matrix:**
   - Hierarchy: `Leader` (5) > `Officer` (4) > `Veteran` (3) > `Member` (2) > `Recruit` (1)
   - **Kicking:** A caller can only kick players of strictly lower rank (`actorRole > targetRole`). Officers cannot kick other Officers or the Leader. No member can kick themselves via the kick RPC (they must call `leave`).
   - **Promotion/Demotion:** Only the `Leader` can promote/demote to or from `Officer`. `Officer` can only manage `Member` and `Recruit` ranks up to `Veteran`.
   - **Notice & Settings:** Only `Leader` and `Officer` can edit the alliance notice or change the join policy.
   - **Disbanding / Leadership Transfer:** Only the `Leader` can transfer leadership or disband the alliance.
4. **Alliance Creation & Economic Deductions:**
   - Creation requires **10,000 Gold**.
   - Creation is executed within an atomic database transaction: verifies player is not in an alliance, verifies tag/name uniqueness, deducts 10,000 Gold from `player_wallets`, creates the alliance row, and inserts the player as `Leader`.
5. **Idempotency & Replay Protection:**
   - Every mutating request must supply a client-generated UUIDv4 `request_id`.
   - Server checks `idempotency_records` table with a unique constraint on `(player_id, request_id)`. Replaying an identical `request_id` returns the cached response without duplicate gold deduction, duplicate application, or duplicate member insertion.
6. **Concurrency & Capacity Invariants:**
   - Maximum capacity is hard-coded at **50 members**.
   - When joining or accepting applications, the server acquires a row-level lock (`SELECT ... FOR UPDATE`) on the alliance record to prevent race conditions exceeding 50 members.
   - A player can belong to at most **one** alliance at any time (`UNIQUE (player_id)` on `alliance_members`).

---

## 2. API ENDPOINTS & SCHEMAS

### 2.1 `kww.alliance.get`
Fetches the public profile of a specific alliance or the caller's active alliance.

- **RPC Method:** `kww.alliance.get`
- **Authentication:** Bearer Session Token (Nakama Auth)
- **HTTP/gRPC Mapping:** POST `/v2/rpc/kww.alliance.get`

#### Request Payload:
```json
{
  "alliance_id": "all_4a9b2c8e"
}
```
*(If `alliance_id` is omitted or empty, the server resolves the caller's currently joined alliance).*

#### Response Payload (`200 OK`):
```json
{
  "alliance": {
    "alliance_id": "all_4a9b2c8e",
    "name": "King Vanguard",
    "tag": "KWW",
    "notice": "Welcome to the Vanguard. Coordinate through discord/chat.",
    "leader_player_id": "usr_98a7f21b",
    "leader_player_name": "Commander_Alpha",
    "member_count": 28,
    "max_members": 50,
    "total_combat_power": 12850000,
    "join_policy": 1,
    "created_at_utc": "2026-11-19T10:00:00.000Z"
  },
  "caller_role": 1
}
```

---

### 2.2 `kww.alliance.search`
Searches for alliances by name or tag with optional join policy filters.

- **RPC Method:** `kww.alliance.search`
- **Authentication:** Bearer Session Token
- **HTTP/gRPC Mapping:** POST `/v2/rpc/kww.alliance.search`

#### Request Payload:
```json
{
  "query": "King",
  "policy_filter": 1,
  "limit": 20,
  "offset": 0
}
```

#### Response Payload (`200 OK`):
```json
{
  "alliances": [
    {
      "alliance_id": "all_4a9b2c8e",
      "name": "King Vanguard",
      "tag": "KWW",
      "notice": "Welcome to the Vanguard.",
      "leader_player_id": "usr_98a7f21b",
      "leader_player_name": "Commander_Alpha",
      "member_count": 28,
      "max_members": 50,
      "total_combat_power": 12850000,
      "join_policy": 1,
      "created_at_utc": "2026-11-19T10:00:00.000Z"
    }
  ]
}
```

---

### 2.3 `kww.alliance.get_members`
Returns the complete member roster of an alliance.

- **RPC Method:** `kww.alliance.get_members`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "alliance_id": "all_4a9b2c8e"
}
```

#### Response Payload (`200 OK`):
```json
{
  "members": [
    {
      "player_id": "usr_98a7f21b",
      "player_name": "Commander_Alpha",
      "role": 1,
      "combat_power": 1500000,
      "contribution_score": 12400,
      "joined_at_utc": "2026-11-19T10:00:00.000Z"
    },
    {
      "player_id": "usr_77f4c19d",
      "player_name": "Major_Bravo",
      "role": 2,
      "combat_power": 920000,
      "contribution_score": 8500,
      "joined_at_utc": "2026-11-19T11:15:00.000Z"
    }
  ]
}
```

---

### 2.4 `kww.alliance.get_applications`
Returns pending membership applications for the alliance. Restricted to `Leader` and `Officer`.

- **RPC Method:** `kww.alliance.get_applications`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "alliance_id": "all_4a9b2c8e"
}
```

#### Response Payload (`200 OK`):
```json
{
  "applications": [
    {
      "application_id": "app_91238491",
      "alliance_id": "all_4a9b2c8e",
      "player_id": "usr_88a9c23f",
      "player_name": "Recruit_Delta",
      "combat_power": 450000,
      "applied_at_utc": "2026-11-19T14:20:00.000Z"
    }
  ]
}
```

---

### 2.5 `kww.alliance.create`
Deducts 10,000 Gold and creates a new alliance with the caller as Leader.

- **RPC Method:** `kww.alliance.create`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Desert Falcons",
  "tag": "DES",
  "notice": "Elite desert recon unit.",
  "join_policy": 1
}
```

#### Response Payload (`200 OK`):
```json
{
  "alliance": {
    "alliance_id": "all_6f1a8e99",
    "name": "Desert Falcons",
    "tag": "DES",
    "notice": "Elite desert recon unit.",
    "leader_player_id": "usr_98a7f21b",
    "leader_player_name": "Commander_Alpha",
    "member_count": 1,
    "max_members": 50,
    "total_combat_power": 850000,
    "join_policy": 1,
    "created_at_utc": "2026-11-19T14:45:00.000Z"
  }
}
```

---

### 2.6 `kww.alliance.join`
Directly joins an alliance with `Open` join policy (`join_policy = 1`).

- **RPC Method:** `kww.alliance.join`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "alliance_id": "all_4a9b2c8e"
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "alliance_id": "all_4a9b2c8e",
  "assigned_role": 5
}
```

---

### 2.7 `kww.alliance.apply`
Submits an application to join an alliance with `ApplyOnly` join policy (`join_policy = 2`).

- **RPC Method:** `kww.alliance.apply`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "b2c3d4e5-f6a7-8901-bcde-f23456789012",
  "alliance_id": "all_4a9b2c8e"
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "application_id": "app_91238491"
}
```

---

### 2.8 `kww.alliance.application.decide`
Accepts or rejects a pending application. Restricted to `Leader` and `Officer`.

- **RPC Method:** `kww.alliance.application.decide`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "c3d4e5f6-a7b8-9012-cdef-345678901234",
  "alliance_id": "all_4a9b2c8e",
  "application_id": "app_91238491",
  "decision": "ACCEPT"
}
```
*(Decision: `"ACCEPT"` or `"REJECT"`)*

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "application_id": "app_91238491",
  "status": "ACCEPTED"
}
```

---

### 2.9 `kww.alliance.kick`
Removes a lower-ranked member from the alliance.

- **RPC Method:** `kww.alliance.kick`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "d4e5f6a7-b8c9-0123-defa-456789012345",
  "alliance_id": "all_4a9b2c8e",
  "target_player_id": "usr_88a9c23f"
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "kicked_player_id": "usr_88a9c23f"
}
```

---

### 2.10 `kww.alliance.role.set`
Promotes or demotes an existing member.

- **RPC Method:** `kww.alliance.role.set`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "e5f6a7b8-c9d0-1234-efab-567890123456",
  "alliance_id": "all_4a9b2c8e",
  "target_player_id": "usr_77f4c19d",
  "new_role": 2
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "target_player_id": "usr_77f4c19d",
  "new_role": 2
}
```

---

### 2.11 `kww.alliance.leave`
Allows a non-leader member to voluntarily leave an alliance.

- **RPC Method:** `kww.alliance.leave`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "f6a7b8c9-d0e1-2345-fabc-678901234567",
  "alliance_id": "all_4a9b2c8e"
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "left_alliance_id": "all_4a9b2c8e"
}
```

---

### 2.12 `kww.alliance.notice.update`
Updates the alliance public notice. Restricted to `Leader` and `Officer`.

- **RPC Method:** `kww.alliance.notice.update`
- **Authentication:** Bearer Session Token

#### Request Payload:
```json
{
  "request_id": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "alliance_id": "all_4a9b2c8e",
  "notice": "All units prepare for Territory Skirmish at 18:00 UTC."
}
```

#### Response Payload (`200 OK`):
```json
{
  "success": true,
  "updated_notice": "All units prepare for Territory Skirmish at 18:00 UTC."
}
```

---

## 3. POSTGRESQL DATABASE SCHEMA

```sql
-- Alliances Master Table
CREATE TABLE IF NOT EXISTS alliances (
    alliance_id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(24) NOT NULL UNIQUE,
    tag VARCHAR(4) NOT NULL UNIQUE,
    notice VARCHAR(256) NOT NULL DEFAULT '',
    leader_player_id VARCHAR(64) NOT NULL,
    member_count INT NOT NULL DEFAULT 1 CHECK (member_count >= 1 AND member_count <= 50),
    max_members INT NOT NULL DEFAULT 50,
    total_combat_power BIGINT NOT NULL DEFAULT 0 CHECK (total_combat_power >= 0),
    join_policy INT NOT NULL DEFAULT 1 CHECK (join_policy IN (1, 2, 3)), -- 1: Open, 2: ApplyOnly, 3: Closed
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_alliances_tag ON alliances(tag);
CREATE INDEX IF NOT EXISTS idx_alliances_name ON alliances(name);
CREATE INDEX IF NOT EXISTS idx_alliances_join_policy ON alliances(join_policy);

-- Alliance Members Table
CREATE TABLE IF NOT EXISTS alliance_members (
    alliance_id VARCHAR(64) NOT NULL REFERENCES alliances(alliance_id) ON DELETE CASCADE,
    player_id VARCHAR(64) NOT NULL UNIQUE, -- Player can belong to only one alliance
    player_name VARCHAR(32) NOT NULL,
    role INT NOT NULL CHECK (role IN (1, 2, 3, 4, 5)), -- 1: Leader, 2: Officer, 3: Veteran, 4: Member, 5: Recruit
    combat_power BIGINT NOT NULL DEFAULT 0 CHECK (combat_power >= 0),
    contribution_score BIGINT NOT NULL DEFAULT 0 CHECK (contribution_score >= 0),
    joined_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (alliance_id, player_id)
);

CREATE INDEX IF NOT EXISTS idx_alliance_members_player ON alliance_members(player_id);
CREATE INDEX IF NOT EXISTS idx_alliance_members_alliance ON alliance_members(alliance_id);

-- Alliance Applications Table
CREATE TABLE IF NOT EXISTS alliance_applications (
    application_id VARCHAR(64) PRIMARY KEY,
    alliance_id VARCHAR(64) NOT NULL REFERENCES alliances(alliance_id) ON DELETE CASCADE,
    player_id VARCHAR(64) NOT NULL,
    player_name VARCHAR(32) NOT NULL,
    combat_power BIGINT NOT NULL DEFAULT 0,
    applied_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (alliance_id, player_id) -- Prevent spamming multiple applications to same alliance
);

CREATE INDEX IF NOT EXISTS idx_alliance_applications_alliance ON alliance_applications(alliance_id);

-- Idempotency Records Table
CREATE TABLE IF NOT EXISTS idempotency_records (
    player_id VARCHAR(64) NOT NULL,
    request_id VARCHAR(64) NOT NULL,
    endpoint VARCHAR(64) NOT NULL,
    response_payload JSONB NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (player_id, request_id)
);
```

---

## 4. SECURITY & ANTI-CHEAT AUDIT

1. **Privilege Escalation:**
   - Server strictly verifies caller identity from Nakama session token (never trusts a passed `caller_player_id`).
   - Role checks are performed by querying `alliance_members` row inside the transaction.
   - An Officer cannot promote another member to Officer or Leader.
   - Demoting or kicking an Officer requires caller to be `Leader`.
2. **Transaction Isolation:**
   - When joining or accepting applications, `SELECT member_count FROM alliances WHERE alliance_id = $1 FOR UPDATE` prevents concurrent requests from exceeding 50 members.
   - Creation performs atomic deduction of 10,000 Gold from `player_wallets`; if insufficient, transaction rolls back immediately with `INSUFFICIENT_RESOURCES`.
3. **Data Validation:**
   - Alliance Tag: strictly 3 or 4 uppercase alphanumeric characters (`^[A-Z0-9]{3,4}$`).
   - Alliance Name: 3 to 24 characters, sanitized against control characters.
   - Alliance Notice: maximum 256 characters.
4. **Rate Limiting:**
   - Join/Apply requests rate-limited to 5 requests per minute per player.
   - Search requests rate-limited to 30 requests per minute per player.

---

## 5. ERROR CODES MAPPING

| Error Code | HTTP / RPC Status | Description |
|---|---|---|
| `ALLIANCE_NOT_FOUND` | 404 Not Found | Target alliance does not exist. |
| `ALLIANCE_FULL` | 400 Bad Request | Alliance has reached maximum capacity (50). |
| `ALREADY_IN_ALLIANCE` | 400 Bad Request | Player is already a member of an alliance. |
| `NOT_IN_ALLIANCE` | 400 Bad Request | Player is not a member of any alliance. |
| `INSUFFICIENT_PERMISSIONS` | 403 Forbidden | Caller role cannot perform this action. |
| `CANNOT_KICK_HIGHER_ROLE` | 403 Forbidden | Target member has equal or higher rank. |
| `APPLICATION_NOT_FOUND` | 404 Not Found | Application ID does not exist. |
| `ALLIANCE_TAG_TAKEN` | 409 Conflict | Alliance tag is already registered. |
| `ALLIANCE_NAME_TAKEN` | 409 Conflict | Alliance name is already registered. |
| `INSUFFICIENT_RESOURCES` | 400 Bad Request | Player does not have 10,000 Gold to create an alliance. |
| `DUPLICATE_REQUEST` | 200 OK (Cached) | Idempotent duplicate request; returns cached result. |
| `INVALID_PAYLOAD` | 400 Bad Request | Tag/name format or length violation. |
