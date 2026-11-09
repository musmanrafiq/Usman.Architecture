using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using RedisWriteBehindCounter.Data;

// ─── Demo: Redis write-behind counter pattern ───────────────────────────────

// This demo uses an in-memory SQLite DB for the SQL sink. For production, swap
// to SQL Server with a real connection string:
//   options.UseSqlServer("Server=.;Database=counter_demo;Trusted_Connection=True;");

var builder = new HostBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        // ── Redis (requires a running Redis server, e.g. docker run -d -p 6379:6379 redis:7-alpine) ──
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(hostContext.Configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Redis connection string not configured")));

        // ── EF Core / SQL (SQLite for demo) ──────────────────────────────────────
        services.AddDbContext<CounterContext>(opts =>
            opts.UseSqlite("Data Source=counter_demo.db"));

        // ── Counter services ──────────────────────────────────────────────────────
        services.AddSingleton<ICounterStore, RedisCounterStore>();
        services.AddSingleton<ISqlCounterStore, SqlCounterStore>();
        services.AddHostedService<CounterDrainJob>();
    });

var app = builder.Build();

// Seed some aggregated rows so the dashboard has data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CounterContext>();
    context.Database.EnsureCreated();

    if (!context.CounterAggregateds.Any())
    {
        context.CounterAggregateds.AddRange(new[]
        {
            new CounterAggregated
            {
                CounterType = "api_token_usage",
                Granularity = "day",
                DimensionKey = "tenant_A",
                Value = 120,
                RecordedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new CounterAggregated
            {
                CounterType = "api_token_usage",
                Granularity = "day",
                DimensionKey = "tenant_B",
                Value = 88,
                RecordedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new CounterAggregated
            {
                CounterType = "page_views",
                Granularity = "hour",
                DimensionKey = "tenant_A",
                Value = 1523,
                RecordedAt = DateTimeOffset.UtcNow.AddHours(-2)
            }
        });
        context.SaveChanges();
    }
}

var summary = app.Services.CreateScope().ServiceProvider.GetRequiredService<ICounterStore>();

// ── Simulate your application recording usage ───────────────────────────────
Console.WriteLine("=== Redis Write-Behind Counter Demo ===");
Console.WriteLine();

var recordCount = 0;
var total = 20;
var random = new Random(42);

for (int i = 0; i < total; i++)
{
    var tenant = random.Next(1, 4) switch
    {
        1 => "tenant_A",
        2 => "tenant_B",
        _ => "tenant_C"
    };

    var counterType = random.Next(2) == 0 ? "api_token_usage" : "page_views";
    var granularity = i % 5 == 0 ? "day" : "hour";
    var dimensionKey = counterType == "api_token_usage" ? tenant : "dashboard_page";

    // Each call here simulates a real API endpoint or page view:
    // Instead of hitting SQL on every event, we hit Redis (sub-millisecond).
    var newValue = await summary.IncrementAsync(tenant, counterType, dimensionKey, granularity, count: 1);

    recordCount++;
    if (recordCount % 5 == 0 || i == 0 || i == total - 1)
    {
        Console.WriteLine($"[Event {i + 1}/{total}] {counterType} — {granularity}: {dimensionKey} → Redis now at {newValue}");
    }

    await Task.Delay(50); // Simulate realistic request pacing
}

Console.WriteLine();
Console.WriteLine("All {0} counter events have been recorded into Redis (fast path).", recordCount);
Console.WriteLine();
Console.WriteLine("Drain job is running in the background every 30 seconds (configurable).");
Console.WriteLine("Press Ctrl+C to exit.");
Console.WriteLine();

// Keep the process alive so the background drain job runs and the DB has time to be populated
await Task.Delay(Timeout.Infinite);
