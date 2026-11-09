# ADR-0034: Event versioning and schema evolution for event-driven systems

- Status: Proposed
- Date: 2026-04-01
- By: Usman Rafiq
- Related: ADR-0026 (outbox pattern), ADR-0030 (saga handler boundaries)

## Context

In event-driven architectures, events are the **contract** between producers and consumers:

```csharp
// v1
public record OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

// v2 — new field added
public record OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string PaymentMethod { get; set; }  // NEW
}
```

### The problem

When you change an event schema:

- **Breaking change**: Consumers can't deserialize old messages without upgrading
- **No backward compatibility**: You must deploy all consumers simultaneously, or the pipeline breaks
- **Broadcast deployment risk**: In a distributed system, you can't coordinate all consumers

### Constraints

- Must support **backward compatibility** (old consumers read new events)
- Must support **forward compatibility** (new consumers read old events)
- Must be versioned in a sustainable way
- Must not couple event schema to the domain model

## Decision

We will implement **schema versioning using a version field in the event payload** (not through a different type).

### Event schema design

```json
// Event envelope (same for all versions)
{
    "eventId": "3b8e4a1f-9c2d-4e5f-a6b7-8c9d0e1f2a3b",
    "eventType": "com.company.orders.OrderCreated",
    "eventVersion": 1,
    "occurredAt": "2026-04-01T12:00:00Z",
    "payload": {
        "orderId": "550e8400-e29b-41d4-a716-446655440000",
        "amount": 100.00,
        "currency": "USD",
        "paymentMethod": "credit_card"  // only in v2+
        "shippingAddress": { ... }
    }
}
```

### Versioning rules

| Change | Version | Backward Compatible? | How to implement |
|--------|---------|----------------------|------------------|
| Add optional field | v2 | ✅ Yes | Consumer checks version < 2 |
| Remove field | v2 (deprecate) | ❌ No | Use 6-month deprecation window |
| Change field type | v2 | ❌ No | Cannot safely do without breaking |
| Change field meaning | v2 | ❌ No | Breaking change; needs new version |
| Add new event type | v1 → new type | ✅ Yes | No schema change |

### Implementation: Version-aware deserialization

```csharp
public class EventDeserializer
{
    public IEvent Deserialize(byte[] bytes, int version)
    {
        return version switch
        {
            1 => JsonSerializer.Deserialize<OrderCreatedEventV1>(bytes),
            2 => JsonSerializer.Deserialize<OrderCreatedEventV2>(bytes),
            _ => throw new NotSupportedException($"Version {version} not supported")
        };
    }
}

public class OrderCreatedEventV1
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public class OrderCreatedEventV2
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string PaymentMethod { get; set; }  // v2 adds this
}
```

### Migration strategy

| Phase | Action |
|-------|--------|
| **1** | Deploy v1 producers (no new events) |
| **2** | Deploy v2 producers (writers) — consumers must handle v1 |
| **3** | After 6-month deprecation, remove v1 from schema |

## Alternatives considered

### Option A: Use a shared event library (one common package)

- **Pros**: Single source of truth
- **Cons**: Tight coupling; consumers must upgrade together; version conflicts
- **Verdict**: Rejected

### Option B: Use Apache Avro / Protobuf (schema registry)

- **Pros**: Enforces schema evolution rules; binary format
- **Cons**: Additional infrastructure; complex to use; not as flexible as JSON
- **Verdict**: Chosen as follow-up for high-throughput systems

### Option C: Message version in contract, no schema evolution

- **Pros**: Simple
- **Cons**: No evolution; must retire and recreate events
- **Verdict**: Rejected

## Consequences

### ✅ Good

- **Safe evolution** — producers can deploy independently of consumers
- **Older consumers keep working** — they ignore new fields
- **Cleaner monitoring** — schema versions visible in dashboards
- **No dual-deploy risk** — producers and consumers can deploy separately

### ❌ Bad

- **More complex code** — version-aware deserialization is harder to maintain
- **Testing overhead** — must test each version scenario
- **Metric noise** — multiple schema versions in the system

### 🔧 Follow-ups

- Implement **schema validation** (JSON Schema or equivalent) per version
- Add **schema registry** (optional) for binary formats (Avro/Protobuf)
- Create **deprecation warnings** in logs when v1 producer emits v1 events
- Test **each consumer version** against each producer version

## References

- ADR-0026: Outbox pattern for reliable event publishing
- ADR-0030: Saga handler single-purpose boundaries

## Architect Note

> In event-driven systems, **the event is the API**.  
> If you treat events like a telephone call that goes away after you hang up, you'll be fine.  
> If you treat events like a published contract, you need versioning, or the contract will break.
