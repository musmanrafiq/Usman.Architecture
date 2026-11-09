# ADR-0035: Feature flags for progressive delivery

- Status: Proposed
- Date: 2026-04-01
- By: Usman Rafiq
- Related: ADR-0031 (API rate limiting), ADR-0030 (saga handler boundaries)

## Context

Modern software delivery requires **controlled exposure** of new functionality:

- **Canary releases** — ship to 1% of users, monitor for errors
- **A/B testing** — compare two implementations
- **Pre-release validation** — let internal users test before GA
- **Emergency kill-switch** — disable a feature without a deployment

### The problem

Without feature flags:

- To deploy new functionality, you must deploy code
- Deploying code is risky; you can't easily undo a partial rollout
- Teams cannot validate functionality in production before GA
- Emergency disabling requires a new deployment (too slow)

### Constraints

- Flags must be **dynamic** (can change without a redeploy)
- Must support **segmentation** (by tenant, user, path, role)
- Must be **observable** (see who is affected, how they behave)
- Must be **safe** (fail-closed; if flag service is down, feature is off)

## Decision

We will use a **flag registry** with a **runtime-evaluated, fail-closed** design:

### Flag data model

```sql
CREATE TABLE FeatureFlags (
    Id BIGINT IDENTITY PRIMARY KEY,
    Key VARCHAR(100) NOT NULL,          -- e.g., "new_checkout_flow"
    Name VARCHAR(200) NOT NULL,
    Enabled BIT NOT NULL DEFAULT 0,     -- Fallback: if flag service is down
    RolloutPercentage INT NOT NULL DEFAULT 0,  -- % of users to see the flag
    TargetTiers VARCHAR(100),           -- e.g., "admin,internal"
    CreatedBy VARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
);
```

### Evaluation rules

```
Flag evaluation order:
1. Is the flag enabled in the registry? → No → OFF
2. Is the user in a target tier? → Yes → ON
3. Is rollout percentage >= random(0,100)? → Yes → ON
4. Otherwise → OFF
```

### Example: checking a flag

```csharp
// In a handler or service
public async Task<bool> ShouldShowNewCheckout()
{
    var flag = await _flagStore.GetFlagAsync("new_checkout_flow");
    if (!flag.Enabled) return false;
    if (await _userStore.IsInTargetTier(User, flag.TargetTiers)) return true;
    var random = new Random();
    return random.Next(0, 100) < flag.RolloutPercentage;
}
```

### Registry as a service

```
Flag registry (Redis / distributed cache)
    ↓
Runtime resolution (ASP.NET Core middleware)
    ↓
Flag evaluation (service)
    ↓
Code path (handler)
```

## Alternatives considered

### Option A: Hard-coded boolean in code

- **Pros**: Simple
- **Cons**: Deploy to change; no validation; no kill-switch
- **Verdict**: Rejected

### Option B: Feature flags in config file (appsettings.json)

- **Pros**: Easy to set
- **Cons**: Requires redeploy; no runtime updates; no segmentation
- **Verdict**: Rejected

### Option C: Use a third-party flag service (LaunchDarkly, ConfigCat, etc.)

- **Pros**: Managed, audited, has UI for toggling
- **Cons**: Vendor lock-in; cost per user; privacy concerns
- **Verdict**: Rejected (need self-managed for architecture notebook)

### Option D: Self-managed flag registry with ASP.NET Core middleware

- **Pros**: Full control; runtime updates; segmentation
- **Cons**: More infrastructure; cache invalidation; monitoring
- **Verdict**: Chosen

## Consequences

### ✅ Good

- **Safer deployments** — canary, rollback without redeploy
- **Faster validation** — test in production with real users
- **Emergency control** — kill-switch works immediately
- **Better analytics** — see which flags are used, by whom, and how

### ❌ Bad

- **More code complexity** — each flag adds conditional logic
- **Testing burden** — must test with flags on and off
- **Technical debt** — orphaned flags accumulate over time
- **Performance** — flag resolution adds latency (mitigated by caching)

### 🔧 Follow-ups

- Add **flag health check** (monitoring if registry is reachable)
- Implement **flag usage tracking** (metrics by flag, user, endpoint)
- Create **flag cleanup job** (disable orphaned flags after 90 days)
- Add **automated rollbacks** for failing canary tests

## References

- ADR-0031: API rate limiting
- ADR-0027: Observability with OpenTelemetry
- ADR-0029: Database migration strategy

## Architect Note

> Feature flags are not a "nice to have" — they are **how you safely ship in production**.  
> If you don't have them, every deployment is a risk.  
> If you do, you gain the power to roll back without redeploy.
