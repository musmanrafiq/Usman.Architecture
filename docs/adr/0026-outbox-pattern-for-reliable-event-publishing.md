# ADR-0026: Outbox pattern for reliable event publishing

## Context

When a domain module **publishes domain events** after a state change (e.g., "OrderCreated", "PaymentProcessed"),
those events must be:

1. Persisted alongside the main transaction (no partial failure)
2. Published to the message broker **exactly once**
3. Guaranteed to be re-processed if the broker is temporarily unavailable

### The naive approach

```csharp
// BAD: separate transaction for event publication
await _dbContext.SaveChangesAsync();          // DB transaction
await _eventBus.PublishAsync(event);          // External call — may fail or double-publish
## Decision

We will implement the **Transactional Outbox Pattern** in every module that publishes domain events.

### Architecture

```
┌─────────────┐          ┌──────────────┐          ┌──────────────┐
│  Domain     │          │   DB         │          │  Publisher   │
│  Service    │          │              │          │  (batch job) │
│             │          │  ┌──────────┐│          │              │
│  1. Save    │─────────►│  │ Outbox   ││          │  1. Scan     │
│     (DB txn) │          │  │ rows     ││          │     Outbox │
│             │          │  └──────────┘│          │  2. Publish  │
│  2. Item    │          │              │          │  3. Delete   │
│     saved   │          └──────────────┘          └──────────────┘
└─────────────┘
```

### How it works
### Data model

```sql
CREATE TABLE OutboxMessages (
    Id BIGINT IDENTITY PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    EventType VARCHAR(100) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    Headers NVARCHAR(MAX) NOT NULL,           -- JSON: metadata, correlation IDs
### Option B: Transactional outbox with single-message publishing

- **Pros**: Guaranteed consistency; no race conditions
- **Cons**: Blocks the user request; not suitable for high-throughput scenarios
- **Verdict**: Rejected for high-throughput (use batched outbox)

### Option C: Outbox with a publisher service (dedicated processing)

- **Pros**: Decouples publication latency; allows batch processing; handles retries gracefully
- **Cons**: Additional infrastructure; requires monitoring
- **Verdict**: Chosen

### Option D: Event store / Event sourcing

- **Pros**: Perfect audit trail; can replay any state at any point in history
- **Cons**: Significant paradigm shift; requires new team skills; overkill for most use cases
- **Verdict**: Deferred for use cases that genuinely need event sourcing

## Consequences
### ❌ Bad

- Additional DB row per event (minor overhead)
- Need for a **publisher job** to process outbox rows
- Events must be **serialized/deserialized** (JSON/Avro/Protobuf)
- At-least-once delivery may cause **duplicate processing** — idempotent handlers required
- Slightly more complex testing (db + broker + publisher)

### 🔧 Follow-ups

- Implement **idempotent message handlers** (check `MessageId` in a deduplication table)
- Add monitoring: `outbox_pending_count`, `outbox_publish_latency`
- Implement **dead-letter queue** for events that repeatedly fail (max 3 retries)
- Schedule publisher job at least as often as event burst rate / 10

## References

- ADR-0002: How this repository is structured
- ADR-0011: Module boundary rules
- ADR-0025: API versioning strategy

## Architect Note

> The outbox pattern is the **gold standard** for making event-driven systems reliable without 2PC.  
> If your module publishes events, it must also persist them in the outbox.  
> If you skip this, you're designing for "it works until the broker is down."


### ✅ Good

- **Exactly-once semantics** at the domain level (idempotent handlers for duplicates)
- **No lost events** — every state change is published
- **No coupling to message broker availability** — events are persisted first
- **Audit trail** of all events published
- **Manual retry** capability for failed events
- Works with **any message broker** (RabbitMQ, Azure Service Bus, Kafka, etc.)

    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending | Published | Failed
    Attempts INT NOT NULL DEFAULT 0,
    LastFailedAt DATETIME2 NULL,
    PublishedAt DATETIME2 NULL,
    FailedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Outbox_EventId UNIQUE (EventId)
);

CREATE TABLE OutboxEvents (
    Id BIGINT IDENTITY PRIMARY KEY,
    OutboxId BIGINT NOT NULL REFERENCES OutboxMessages(Id),
    PartitionKey VARCHAR(100) NOT NULL,       -- e.g., tenant ID
    Content NVARCHAR(MAX) NOT NULL,
    SequenceNumber BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_OutboxEvents_Partition_Seq UNIQUE (PartitionKey, SequenceNumber)
);
```

## Alternatives considered

### Option A: Dual-write (DB + broker in same transaction)

- **Pros**: Simple
- **Cons**: Requires two databases; most ORMs can't do 2PC; usually ends with eventual consistency bugs
- **Verdict**: Rejected (not atomic)


1. **Within the same DB transaction**, the domain service:
   - Saves the aggregate state (e.g., `Order`)
   - Saves an outbox row (e.g., `OrderCreatedEvent`)
2. **A background publisher job** (runs every 1–5 seconds) queries outbox rows:
   - Publishes each event to the message broker
   - Marks the outbox row as `Published`
   - Deletes or archives the row after successful ack
3. **At-least-once delivery**: If the broker fails to acknowledge, the row remains and is retried.

```

### The problems

- If the broker is down at the same moment as the DB commit → **event lost**
- If publication succeeds but acknowledgment is lost → **double-publish**
- If publishing is part of the same transaction → **deadlock risk** (DB lock + broker connection)
- No **audit trail** of which events failed and when

### Constraints

- Events must be delivered **at least once** (duplicate detection is acceptable)
- Events must be stored in the **same transaction** as the domain state
- The outbox table must be **queryable** for manual retry
- Publishing must be **asynchronous** (not block the user request)

- Status: Proposed
- Date: 2026-03-31
- By: Usman Rafiq
- Related: ADR-0002 (repository structure), ADR-0011 (module boundaries), ADR-0025 (API versioning)