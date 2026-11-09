# ADR-0033: Dead-letter queue for failed message handling

- Status: Proposed
- Date: 2026-04-01
- By: Usman Rafiq
- Related: ADR-0026 (outbox pattern), ADR-0028 (background jobs)

## Context

In any asynchronous message pipeline, some messages will **always** fail:

- Invalid payload (malformed JSON, missing required field)
- Business rule violation (insufficient funds, duplicate order)
- Dependency failure (external API down, DB unavailable)
- Transient errors (network timeout, temporary lock)

### The problem

Without a dead-letter queue:

- Failed messages are **silently dropped** or **retried indefinitely**
- No visibility into what failed or why
- No way to recover or reprocess manually
- Error rates appear in metrics but can't be acted upon

### Constraints

- Must support at-least-once delivery (some messages will fail)
- Must allow manual inspection and retry of failed messages
- Must not block processing of other messages
- Must be observable and alertable

## Decision

We will implement a **two-phase delivery model** with a **dead-letter queue (DLQ)**:

### Flow

```
                    ┌──────────────┐
                    │   Producer   │
                    └──────┬───────┘
                           │ 1. Publish
                           ▼
                    ┌──────────────┐
                    │ Message Bus  │
                    └──────┬───────┘
                           │ 2. Deliver
                           ▼
                    ┌──────────────┐
                    │  Consumer    │
                    └──────┬───────┘
                           │ 3a. Success?
             ┌─────────────┼─────────────┐
             │ Yes         │ No          │
             ▼             ▼             ▼
    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
    │   ACK      │  │   Retry      │  │  Dead Letter │
    │   (n+1)    │  │   (n<max)    │  │   Queue      │
    └──────────────┘  └──────────────┘  └──────────────┘
             │                                         │
             │ (after max retries reached)             │
             ▼                                         ▼
    ┌──────────────┐                          ┌──────────────┐
    │  Process OK  │                          │  Manual      │
    │  (next msg)  │                          │  Inspection  │
    └──────────────┘                          └──────────────┘
```

### Data model

```sql
CREATE TABLE DeadLetterQueue (
    Id BIGINT IDENTITY PRIMARY KEY,
    OriginalMessageId UNIQUEIDENTIFIER NOT NULL,
    EventType VARCHAR(100) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    ErrorType VARCHAR(100) NOT NULL,
    ErrorMessage NVARCHAR(MAX) NOT NULL,
    FailedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    RetryCount INT NOT NULL DEFAULT 0,
    RetryReason NVARCHAR(MAX) NULL,
    ArchivedAt DATETIME2 NULL
);

CREATE TABLE DeadLetterQueueLog (
    Id BIGINT IDENTITY PRIMARY KEY,
    DlqId BIGINT NOT NULL REFERENCES DeadLetterQueue(Id),
    Action VARCHAR(50),          -- Processed | ManuallyRetried | Abandoned
    Details NVARCHAR(MAX) NULL,
    PerformedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

### Retry policy

| Phase | Action | Limit |
|-------|--------|-------|
| **1** | Immediate retry | n/a |
| **2** | Exponential backoff | 30s, 1m, 5m, 15m |
| **3** | Dead letter | After 4 attempts |

### Handling DLQ messages

- **Automated**: Parse error type, fix payload, resubmit
- **Manual**: Admin UI to inspect, fix, and retry
- **Monitoring**: DLQ depth, error rate, retry rate

## Alternatives considered

### Option A: No DLQ — just retry forever

- **Pros**: Simple
- **Cons**: Infinite retries block processing; no visibility; eventually causes queue backlog
- **Verdict**: Rejected

### Option B: Automatic retry with predicate (only retry transient errors)

- **Pros**: Reduces noise
- **Cons**: Still no DLQ for non-transient errors
- **Verdict**: Rejected as primary strategy

### Option C: DLQ with manual inspection only

- **Pros**: No false alarms from automated retries
- **Cons**: Manual work is expensive
- **Verdict**: Rejected — use both auto and manual

### Option D: DLQ with automated remediation for known error types

- **Pros**: Reduces manual work
- **Cons**: Complex to implement correctly
- **Verdict**: Rejected (doable, but keep as follow-up)

## Consequences

### ✅ Good

- **No lost messages** — every failure lands somewhere observable
- **Actionable errors** — DLQ contents tell you what went wrong
- **Recovery** — manual or automated retry of failed messages
- **Pipeline resilience** — failures don't block the entire bus

### ❌ Bad

- **Storage growth** — DLQ requires disk space; monitor depth
- **Manual work** — some messages will require human intervention
- **Complex retry logic** — must be correctly implemented
- **Monitoring burden** — need alerts for DLQ depth > limit

### 🔧 Follow-ups

- Create **data validation** middleware for DLQ messages
- Add **DLQ depth alert** (> 100, > 500, > 1000)
- Implement **DLQ compaction** (archive old messages)
- Add **DLQ analytics** (why do messages fail? which types?)

## References

- ADR-0026: Outbox pattern for reliable event publishing
- ADR-0028: Background job processing with Quartz
- ADR-0032: Idempotency keys for safe retry

## Architect Note

> The dead-letter queue is your **system's conscience**.  
> If you can't see what failed, you can't fix it.  
> If you can't fix it, you're not resilient — you're just lucky.
