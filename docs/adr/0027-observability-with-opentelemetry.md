# ADR-0027: Observability with OpenTelemetry (structured logging, metrics, tracing)

- Status: Proposed
- Date: 2026-03-31
## Context

## Decision

We will adopt a **three-pillar observability stack** for all .NET services:

| Pillar | Tool | Purpose |
|--------|------|---------|
| **Logs** | Serilog + Seq (or DataDog) | Structured, queryable logs |
| **Metrics** | Prometheus + Grafana (or DataDog) | Time-series metrics |
| **Traces** | OpenTelemetry SDK + collector | Distributed request tracing |

### Core instrumentation (ASP.NET Core)

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService("MyApi", serviceVersion: "1.0.0", deploymentEnvironment: "production"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("MyApp.CustomEvents"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddEntityFrameworkCoreInstrumentation());

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();
```

### Structured logging conventions

```
## Alternatives considered

### Option A: System.Diagnostics (built-in `ILogger` + `Activity`)

- **Pros**: No extra packages; built into .NET
- **Cons**: Limited metrics; harder to configure; less structured logging support
- **Verdict**: Rejected for production (use with OpenTelemetry SDK for better coverage)

### Option B: Log4Net (legacy)

- **Pros**: Mature, familiar to many .NET teams
- **Cons**: No native OpenTelemetry support; DSL-heavy; not cloud-native
- **Verdict**: Rejected (use Serilog with OpenTelemetry)

### Option C: Azure Application Insights only

- **Pros**: Good for Azure-native environments; automatic instrumentation
- **Cons**: Vendor lock-in; expensive for large-scale; less portable
- **Verdict**: Accepted as a store-backend for Serilog but not a first-class citizen

### Option D: No structured logging (just `Console.WriteLine`)

- **Pros**: Simplest
- **Cons**: Unsearchable, uncorrelated, cannot ship to production
- **Verdict**: Rejected (production systems require structured logs)

## Consequences

### ✅ Good

- **Queryable logs**: `Where @Level = 'Error' AND TenantId = 'X'` — fast
- **Request correlation**: every HTTP call has a `traceparent` — trace end-to-end
- **SLO visibility**: latency and error-rate metrics per endpoint, per version, per tenant
- **Performance profiling**: CPU, memory, GC, DB query duration all visible in dashboards
- **Zero-performance penalty** when the telemetry backend is slow (batching + sampling)

### ❌ Bad

- **Eventual consistency** in telemetry (logs may lag behind actual state)
- **Excessive logging** can impact performance and storage costs
- **Sampling** may miss rare but important errors
- **No message bus integration** for async job logs (default in .NET, but check integration)

### 🔧 Follow-ups

- Set up **alerting rules**: error rate > 1%, p95 latency > 500ms, CPU > 80%
- Add **distributed tracing infrastructure** (otel-collector) to all services
- Implement **custom metrics** for business KPIs (e.g., "concurrent_active_users")
- Create a **dashboard** per team/service showing: error rate, p95 latency, throughput

## References

- ADR-0002: How this repository is structured
- ADR-0011: Module boundary rules
- ADR-0025: API versioning strategy

## Architect Note

> Observability is not an afterthought — it is **a first-class architectural concern** that affects:
> - How quickly you can debug production incidents (hours → minutes)
> - How confidently you deploy (more frequent, lower risk)
> - How well you understand capacity and SLOs
> If you can't answer "why did this request fail in production?", you don't have observability.

// CORRECT: structured properties, not string interpolation in the message
logger.LogInformation("User {UserId} created API key for tenant {TenantId}", userId, tenantId);

// WRONG: interpolating in message (not queryable)
logger.LogInformation($"User {userId} created API key for tenant {tenantId}");
```

### Trace correlation across services

Every incoming request gets a `traceparent` header (W3C Trace Context). We propagate it:

```csharp
// In a minimal API handler
app.MapGet("/api/v1/orders", async (HttpContext ctx, IOrderService service) =>
{
    // OpenTelemetry automatically traces this via AspNetCoreInstrumentation
    var orders = await service.GetOrdersAsync(ctx.GetRequestedTenantId());
    return Results.Ok(orders);
});
```

### Metrics to emit

| Metric | Type | Purpose |
|--------|------|---------|
| `http_server_requests_seconds` | Histogram | Request latency by endpoint |
| `http_server_requests_total` | Counter | Request count by status code |
| `processor_cpu_usage` | Gauge | CPU utilization |
| `process_working_set_bytes` | Gauge | Memory usage |
| `db_command_duration_seconds` | Histogram | EF Core query latency |

Modern .NET systems run in containerized or serverless environments where **log files are not enough**. We need:

- **Structured logs**: machine-readable, queryable, correlated to a request
- **Metrics**: counters, gauges, histograms for capacity planning and SLOs
- **Traces**: end-to-end request tracking across services

### The problem

The most common mistakes in .NET observability:

- `Console.WriteLine` in production — lost, unstructured, impossible to query
- Structured logging with caller-line numbers (`LogInformation("x", line)`) — noise
- No correlation IDs — impossible to trace a single request across services
- Metrics collected but not exposed to a dashboard
- Distributed tracing disabled or using rest calls that bypass it

### Constraints

- Must work with .NET 8+ minimal APIs and ASP.NET Core
- Must support **structured logging** (Serilog or similar)
- Must emit **OpenTelemetry** traces and metrics
- Must not degrade request latency if the backend is slow
- Must be configurable via `appsettings.json` / environment variables

- By: Usman Rafiq
- Related: ADR-0002 (repository structure), ADR-0011 (module boundaries), ADR-0025 (API versioning)