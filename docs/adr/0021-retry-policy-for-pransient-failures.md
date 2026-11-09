# ADR-0021: Retry Policy for Transient Failures

- Status: Proposed
- Date: 2026-03-29
- By: Usman Rafiq
- Related:
  - /architecture/resilience/retry/RetryPolicyExample.cs
  - ADR-0002 (Circuit Breaker)
  - ADR-0006 (Composite Resilience Strategy)

## Context

The system integrates with external services such as:

Payment gateways
Third-party APIs
Internal services over HTTP

These dependencies are unreliable by nature and may fail due to:

Network instability
Temporary service downtime
DNS resolution issues
Rate limiting (HTTP 429)

These failures are often transient, meaning retrying after a short delay can succeed.

Constraints:

Must not impact system stability
Must avoid excessive retries (retry storms)
Must support async operations
Should be easy to standardize across services

## Decision

We will implement a retry mechanism using Polly to handle transient failures in external calls.

The retry policy will:

Retry up to 3 times
Use exponential backoff
Handle both exceptions and unsuccessful HTTP responses

## Alternatives considered

Option A: No Retry
Simpler implementation
❌ Leads to fragile system
❌ Poor user experience on transient failures

Option B: Custom Retry Logic (Manual Implementation)
Full control over behavior
❌ Reinvents the wheel
❌ Harder to maintain and standardize
❌ Lacks advanced features like jitter, policy composition

Option C: Use .NET Built-in Resilience (e.g., HttpClientFactory handlers only)
Native integration
❌ Less flexible compared to Polly
❌ Harder to express advanced policies cleanly

## Consequences

✅ Good
Improves system reliability
Handles transient failures gracefully
Standardized retry behavior across services
Easy to extend and combine with other policies

❌ Bad
Increased latency due to retries
Risk of retry storms under high failure rates
Can overload downstream services if misconfigured

🔧 Follow-ups
Combine with Circuit Breaker (ADR-0002) to prevent cascading failures
Add logging for retry attempts
Introduce jitter to reduce retry spikes
Monitor retry metrics (count, failure rate)

## References

Polly documentation (official)
/architecture/resilience/retry/RetryPolicyExample.cs
/samples/aspnet-core/http-retry-demo

## Architect Note (Important)

Retries must only be used for idempotent operations.

Do NOT apply retry blindly to:

Payment processing
Order creation
Any operation with side effects
