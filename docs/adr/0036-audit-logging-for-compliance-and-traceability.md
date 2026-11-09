# ADR-0036: Audit logging for compliance and traceability

- Status: Proposed
- Date: 2026-04-01
- By: Usman Rafiq
- Related: ADR-0027 (observability), ADR-0011 (module boundaries)

## Context

Many industries and domains require **audit trails**:

- **Financial**: who approved a payment, when, and why?
- **Healthcare**: patient records access, changes to medical data
- **Compliance**: show who changed what and when
- **Security**: detect unauthorized access or suspicious activity

### The problem

Without audit logging:

- No evidence of who did what
- Cannot prove compliance to auditors
- Cannot trace a problem back to its source
- Security incidents become unprovable

### Constraints

- Must capture **who** (user/message), **what** (action), **when** (timestamp), **where** (system, IP)
- Must distinguish between **audit events** (compliance) and **error logs** (debug)
- Must not be affected by application code changes (stable, predictable)
- Must be **tamper-evident** (immutable, append-only for compliance)

## Decision

We will implement **structured audit logging** as a separate concern from application logging.

### Audit event structure

```json
{
    "eventId": "a1b2c3d4-...",
    "timestamp": "2026-04-01T12:00:00Z",
    "userId": "user_123",
    "tenantId": "tenant_A",
    "action": "PaymentApproved",
    "resourceType": "Payment",
    "resourceId": "PAY-98765",
    "oldValue": { "status": "Pending" },
    "newValue": { "status": "Approved" },
    "actorIp": "10.0.0.1",
    "userAgent": "Mozilla/5.0...",
    "actorRole": "FinanceAdmin",
    "metadata": { "approvedBy": "john.doe@company.com" }
}
```

### Implementation: Audit logging middleware

```csharp
// In Program.cs — audit middleware captures domain events
app.Use(async (context, next) =>
{
    await next();

    // After each request, check if an audit event was created
    if (context.Items["AuditEvent"] is AuditEvent audit)
    {
        await _auditWriter.WriteAsync(audit);
    }
});
```

### Auditor types

| Audit Type | Scope | Example |
|------------|-------|---------|
| **User action** | Who did something | Login, payment approval, data export |
| **System event** | Who/what did something | Scheduled job, backup, migration |
| **Access event** | Who accessed what | Viewing patient records, admin access |

### Compliance requirements

- **Immutable**: Audit logs cannot be edited or deleted
- **Append-only**: New entries only, never modify past entries
- **Tamper-evident**: Cryptographic hash of previous entry stored in next entry
- **Retention**: 7 years for financial, 3 years for healthcare (per regulation)

### Audit log sink

| Sink | Use Case |
|------|----------|
| **WORM storage** (Write Once Read Many) | Financial compliance (7 years) |
| **S3 with object lock** | Cloud-native compliant storage |
| **Sequel / Splunk** | Queryable audit logs, fast search |

## Alternatives considered

### Option A: Log to the same log as application logs

- **Pros**: Simple
- **Cons**: Confusing; no separation; hard to enforce retention
- **Verdict**: Rejected

### Option B: Separate audit table in DB

- **Pros**: Queryable, fast
- **Cons**: Not immutable; can be edited by buggy code
- **Verdict**: Rejected (use append-only sink for compliance)

### Option C: Event sourcing (already has history)

- **Pros**: Natural audit trail
- **Cons**: Not suitable for all domains; significant complexity
- **Verdict**: Deferred for use cases that already have event sourcing

## Consequences

### ✅ Good

- **Compliance** — can prove who did what to whom
- **Traceability** — trace any action back to its source
- **Security** — detect unauthorized access or suspicious changes
- **Accountability** — teams know actions are tracked

### ❌ Bad

- **Storage costs** — audit logs are append-only; long retention
- **Performance overhead** — writing audit events adds latency
- **Privacy concerns** — audit logs may contain sensitive data (PII)
- **Complexity** — must aggregate audit events across services

### 🔧 Follow-ups

- Implement **cryptographic chaining** (hash of previous entry)
- Set up **audit log retention policies** (7 years for financial, 3 for healthcare)
- Create **audit log search UI** for compliance teams
- Add **PII detection** for sensitive data (PII scrubbing)

## References

- ADR-0027: Observability with OpenTelemetry
- ADR-0029: Database migration strategy
- ADR-0011: Module boundary rules

## Architect Note

> Audit logs are not for **you** — they are for **a future auditor** who asks:  
> "Who changed this?"  
> If you can't answer, your systems lack accountability.
