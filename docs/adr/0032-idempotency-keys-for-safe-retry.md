# ADR-0032: Idempotency keys for safe retries

- Status: Proposed
- Date: 2026-04-01
- By: Usman Rafiq
- Related: ADR-0026 (outbox pattern), ADR-0030 (saga handler boundaries)

## Context

When systems communicate over HTTP, networks are unreliable:

- **Timeout**: The client didn't get a response — it can't tell if the request succeeded
- **Network partition**: The request may have been processed but the response was lost
- **Crash**: The server processed the request but crashed before responding

### The problem

Without idempotency, a retry can cause **duplicate side effects**:

```csharp
// BAD: no idempotency — retry doubles the charge
POST /payments
{
    "amount": 100.00,
    "currency": "USD"
}
// (HTTP 200 OK)

// Network error...

// Client retries...
POST /payments
{
    "amount": 100.00,
    "currency": "USD"
}
// (HTTP 200 OK)

// Result: TWO payments of $100!
```

### Constraints

- Must work for HTTP APIs
- Must handle duplicate detection across distributed services
- Must be configurable per endpoint (some operations are naturally idempotent)
- Must not degrade user experience

## Decision

We will implement **Idempotency-Key headers** for all non-idempotent operations.

### Idempotency-Key semantics (RFC 9457 / IETF draft)

```http
POST /payments
Idempotency-Key: 3b8e4a1f-9c2d-4e5f-a6b7-8c9d0e1f2a3b
Content-Type: application/json

{
    "amount": 100.00,
    "currency": "USD"
}
```

### Behavior

| Scenario | Response |
|----------|----------|
| First request | Execute, store key, return 200 |
| Duplicate request (same key) | Return original 200 + body (no re-execution) |
| Different body with same key | Return `409 Conflict` (key mismatch) |
| Key too old (> 24h) | Return `410 Gone` |

### Storage

```sql
CREATE TABLE IdempotencyKeys (
    Id BIGINT IDENTITY PRIMARY KEY,
    RequestId UNIQUEIDENTIFIER NOT NULL,
    RequestKey VARCHAR(64) NOT NULL,        -- The idempotency key
    RequestBody NVARCHAR(MAX) NOT NULL,    -- Serialized request body
    ResponseBody NVARCHAR(MAX) NOT NULL,   -- Serialized response
    StatusCode INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt DATETIME2 NOT NULL,           -- TTL = 24h (configurable)
    CONSTRAINT UQ_IdempotencyKeys_Key UNIQUE (RequestKey)
);
```

### Endpoint classification

| Endpoint | Idempotent? | Reason |
|----------|-------------|--------|
| `GET /orders` | ✅ Yes | No side effects |
| `POST /orders` | ❌ No | Creates a new order |
| `DELETE /orders/{id}` | ✅ Yes | Deleting a non-existent order is safe |
| `POST /payments` | ❌ No | Charges money — MUST have idempotency |

## Alternatives considered

### Option A: Unique constraint on order number

- **Pros**: Simple, guaranteed uniqueness
- **Cons**: Requires a unique business key (not always available)
- **Verdict**: Rejected (not always applicable)

### Option B: Idempotency-Key in DB transaction

- **Pros**: Guaranteed atomicity
- **Cons**: Long-lived transactions; connection pool pressure
- **Verdict**: Rejected (use outbox pattern for atomicity)

### Option C: Client-side retry with jQuery .post() + success callback

- **Pros**: Simple
- **Cons**: Client-dependent; no server enforcement
- **Verdict**: Rejected (server must enforce)

## Consequences

### ✅ Good

- **Safe retries** — no duplicate side effects
- **Better UX** — users can retry without fear of duplicates
- **Debugging** — can trace a request key to see its full lifecycle
- **Production resilience** — network issues become non-fatal

### ❌ Bad

- **Storage overhead** — need to store request/response for 24h
- **Memory pressure** — caching request bodies and responses
- **Key management** — clients must generate unique keys
- **Requires coordination** — all retry-aware clients must use the same protocol

### 🔧 Follow-ups

- Set up **expiration** for idempotency keys (TTL, cleanup job)
- Add **metrics** for duplicate request rate
- Use **SHA-256** of key for storage efficiency
- Document **best practices** for client SDKs (how to generate keys)

## References

- ADR-0026: Outbox pattern for reliable event publishing
- ADR-0030: Saga handler single-purpose boundaries

## Architect Note

> Idempotency is not an optimization — it's **a correctness requirement** for distributed systems.  
> If your API doesn't handle retries safely, you haven't designed for the real world.
