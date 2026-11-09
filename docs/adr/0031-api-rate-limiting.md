# ADR-0031: API rate limiting for public endpoints

- Status: Proposed
- Date: 2026-04-01
- By: Usman Rafiq
- Related: ADR-0025 (API versioning strategy), ADR-0027 (observability), ADR-0026 (outbox pattern)

## Context

Public-facing APIs face several threats:

- **Traffic spikes** from misbehaving clients or DoS attacks
- **Unfair usage** from a single client consuming disproportionate resources
- **Misconfigured clients** making excessive requests
- **Cost control** — each request consumes compute, database, and external service capacity

### The problem

Without rate limiting:
- A single client can flood the API, starving others
- Burst traffic can degrade service for everyone
- Abuse is difficult to trace and mitigate
- No defense-in-depth against malicious clients

### Constraints

- Must work per-client, per-endpoint, per-version
- Should be configurable per tenant or client type
- Should not block legitimate users (only abusive ones)
- Must be observable (metrics, alerts)

## Decision

We will implement **token-bucket rate limiting** at the API gateway/edge layer, with **per-tenant quotas** enforced at the application layer.

### Token Bucket Algorithm

| Parameter | Meaning | Example |
|-----------|---------|---------|
| `bucket_size` | Max tokens in bucket | 1000 |
| `refill_rate` | Tokens added per second | 100 |
| `burst_limit` | Max burst allowed | 200 |

### Implementation

```csharp
// ASP.NET Core rate limiting middleware (resilience stack)
builder.Services.AddRateLimiter(options =>
{
    // Global limit: 1000 req/min per IP
    options.GlobalLimiter = httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                WindowDuration = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });

    // Per-tenant limit (from tenant registry)
    options.AddPolicy("tenant-limit", httpContext =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: httpContext.Request.Headers["X-Tenant-ID"],
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 50,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));
});
```

### Response behavior

| Status Code | Meaning |
|-------------|---------|
| `429 Too Many Requests` | Rate limit exceeded; include `Retry-After` header |
| `403 Forbidden` | Client was banned (misbehavior, abuse) |

### Headers to emit

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 842
X-RateLimit-Reset: 1711942800
Retry-After: 45  (when 429 triggered)
```

## Alternatives considered

### Option A: Redis-based sliding window

- **Pros**: Accurate, distributed, easy to scale
- **Cons**: Requires Redis; more complex to tune
- **Verdict**: Chosen for distributed deployments; Redis-based tracking in application

### Option B: Database-based token counting

- **Pros**: Simple, visible in SQL
- **Cons**: High contention on hot counters; not suitable for high throughput
- **Verdict**: Rejected

### Option C: Per-IP limiting at edge (CDN/WAF)

- **Pros**: Offloads API from abuse; simple
- **Cons**: Shared IPs (NAT, proxies), hard to enforce per-tenant
- **Verdict**: Complementary (edge layer) + application layer for per-tenant

## Consequences

### ✅ Good

- **Predictable capacity** — no single client can starve others
- **Security** — self-protects against DoS and abuse
- **Fairness** — all tenants get their share of resources
- **Observable** — rate limit hit rates visible in metrics

### ❌ Bad

- **False positives** — legitimate users might hit limits during bursts
- **Tuning complexity** — need to calibrate per endpoint, per client type
- **Additional infrastructure** — Redis or similar for distributed tracking

### 🔧 Follow-ups

- Add **rate limit monitoring** (hit rate, rejected rate, avg latency)
- Set up **alerts** when limit violations exceed a threshold
- Implement **circuit breaker** on rate-limited endpoints to fail fast
- Configure **per-endpoint limits** (e.g., /payments requires 10/sec, /stats 100/sec)

## References

- ADR-0025: API versioning strategy
- ADR-0027: Observability with OpenTelemetry
- ADR-0029: Database migration strategy

## Architect Note

> Rate limiting is not a feature — it's **infrastructure security**.  
> If you don't control how many requests a client can make, you don't control your own cost.
