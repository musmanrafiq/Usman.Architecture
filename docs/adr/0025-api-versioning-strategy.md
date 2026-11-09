# ADR-0025: API versioning strategy for ASP.NET Core

- Status: Proposed
- Date: 2026-03-31
- By: Usman Rafiq
- Related: ADR-0002 (repository structure), ADR-0011 (module boundaries), ADR-0026 (outbox pattern)

## Context

REST APIs are **public contracts** consumed by multiple clients:
- Web applications (browser)
- Mobile apps (iOS, Android)
- Third-party integrations
- Internal service-to-service communication

### The problem

Without a versioning strategy, you inevitably face:

- Breaking changes accidentally released to live clients
- Different clients on different timelines (e.g., mobile vs web)
- Inability to deprecate fields or change semantics without notice
- "Versionless" APIs that degrade into monolithic, unmanageable endpoints over time

### Constraints

- Must support **backward compatibility** for at least one major version
- Must allow **smooth deprecation** (6–12 months notice)
- Must not couple versioning to the domain layer (cross-cutting concern)
- Must be consistent across all API modules

## Decision

We will use **URL-based versioning** (`/api/v1/...`) for public APIs, with optional **header-based versioning** (`Accept: application/json; v=2`) for internal service-to-service calls.

### Versioning rules

| Rule | Description |
|------|-------------|
| **Major versions** | Only breaking changes increment major version (`v1` → `v2`) |
| **Minor/patch** | Non-breaking additions do NOT increment version (`1.1`, `1.2`) |
| **Deprecation window** | 6–12 months before removal; `/api/v1/` deprecated flag in response headers |
| **Default version** | Latest stable version; media type negotiation via `Accept` header |
| **Contract-first** | Version is defined in a `VersionedApi` contract namespace, not in domain code |
| **All controllers** | Every API module implements versioning consistently |

### Implementation

```csharp
// Program.cs — ASP.NET Core
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // /api/v1/...
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // "v1", "v2"
    options.SubstituteApiVersionInUrl = true;
});
```

### Endpoint naming convention

```
GET    /api/v1/tenants/{tenantId}/api-keys
POST   /api/v1/tenants/{tenantId}/api-keys
GET    /api/v1/tenants/{tenantId}/api-keys/{id}
DELETE /api/v1/tenants/{tenantId}/api-keys/{id}
```

## Alternatives considered

### Option A: URL versioning (`/api/v1/...`)

- **Pros**: Easy to implement, visible in browser URL, cacheable, clear for monitoring
- **Cons**: URL length increases; somewhat redundant with modern routing
- **Verdict**: Chosen for public APIs (visibility and observability)

### Option B: Media type versioning (`Accept: application/vnd.myapi.v1+json`)

- **Pros**: Semantically correct, doesn't bloat URLs, aligns with REST principles
- **Cons**: Harder to test in browser, less visible in monitoring tools, harder to cache edge proxies
- **Verdict**: Chosen for internal service-to-service communication

### Option C: Versioning by query string (`/api/v=1/...`)

- **Pros**: Simple
- **Cons**: Unorthodox, hard to reason about in proxies/CDN, less idiomatic
- **Verdict**: Rejected

### Option D: No versioning (monolithic endpoint with version detection)

- **Pros**: Simpler initially
- **Cons**: Breaking changes force emergency deployments; clients break unexpectedly
- **Verdict**: Rejected — unsustainable long-term

## Consequences

### ✅ Good

- Clear, explicit contracts for each version
- Smooth deprecation cycles (clients get warnings, then migration time)
- Easy to version individual modules independently
- Monitoring and logging can filter by version
- Enables parallel runs of v1 and v2 during migration

### ❌ Bad

- API surface area grows with each version
- Requires a deprecation policy document
- More test cases (at least one test per version endpoint)
- Slightly higher cognitive load for consumers

### 🔧 Follow-ups

- Publish a **deprecation policy** document in the repository
- Add `Deprecation` and `Sunset` headers to responses (RFC 8594)
- Create per-version integration tests in CI
- Monitor `410 Gone` and `406 Not Acceptable` responses as signals for cleanup

## References

- ADR-0002: How this repository is structured
- ADR-0011: Module boundary rules
- ADR-0026: Outbox pattern (event publication in versioned APIs)

## Architect Note

> Versioning is not a technical problem — it is a **contract management problem** that happens to live in your API layer.  
> If you don't have a deprecation policy, you don't have versioning; you have "hope."
