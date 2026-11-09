import os

os.makedirs('/Development/DotNet/Usman Architecture/samples/outbox-persistence/Entities', exist_ok=True)
os.makedirs('/Development/DotNet/Usman Architecture/samples/outbox-persistence/OutboxService', exist_ok=True)
os.makedirs('/Development/DotNet/Usman Architecture/samples/outbox-persistence/IntegrationEvents', exist_ok=True)
os.makedirs('/Development/DotNet/Usman Architecture/samples/outbox-persistence/Program', exist_ok=True)

with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/outbox-persistence.csproj', 'w') as f:
    f.write('''<Project Sdk="Microsoft.NET.Sdk.Web">
with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/Entities/OrderCreatedEvent.cs', 'w') as f:
    f.write('''// ADR-0026 — Outbox Event

namespace OutboxPersistence.Entities;

/// <summary>
/// Integration event that gets persisted to the outbox table.
/// This ensures the event is published atomically with the domain change.
/// </summary>
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
}
''')

with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/Entities/PaymentProcessedEvent.cs', 'w') as f:
    f.write('''// ADR-0026 — Outbox Event

with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/OutboxService/IOutboxPublisher.cs', 'w') as f:
    f.write('''// ADR-0026 — Outbox Publisher Interface

using System.Threading;
using System.Threading.Tasks;

namespace OutboxPersistence.OutboxService;

/// <summary>
/// Interface for publishing events with outbox persistence.
// ADR-0026 — IOrderService interface
namespace OutboxPersistence.OutboxService;

public interface IOrderService
{
    Task PlaceOrderAsync(string tenantId, decimal amount, System.Threading.CancellationToken ct = default);
}

/// Events are stored in the database and published asynchronously.
/// </summary>
public interface IOutboxPublisher
// ADR-0026 — Outbox Publisher Background Service
using Microsoft.EntityFrameworkCore;
using OutboxPersistence.Entities;
using System.Text.Json;

namespace OutboxPersistence.OutboxService;

public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;
with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/README.md', 'w') as f:
    f.write('''# ADR-0026: Outbox Pattern Sample

This sample demonstrates the **Transactional Outbox Pattern** for reliable event publishing in .NET.

## What this sample shows

### 1. Atomic persistence with domain changes
```csharp
public async Task PlaceOrderAsync(string tenantId, decimal amount, CancellationToken ct = default)
{
    // 1. Persist the domain change (e.g., save order to DB)
    //    This is done in a database transaction with the outbox row.

    // 2. Publish event to outbox (atomic with the domain change)
    await _publisher.PublishEventAsync(new OrderCreatedEvent
    {
        OrderId = Guid.NewGuid(),
        TenantId = tenantId,
        Amount = amount,
        Status = "Placed"
    }, ct);
}
```

### 2. At-least-once delivery guarantee
- Events are persisted in the outbox before being consumed by the publisher.
- If the publisher crashes after acknowledging the DB but before the broker, the event is re-published.
- This guarantees no events are lost.

### 3. Idempotent consumers
- Outbox pattern provides at-least-once delivery, which means duplicate events are possible.
- Consumers should be **idempotent**: handle the same event multiple times without side effects.

## Project structure

```
samples/outbox-persistence/
├── outbox-persistence.csproj
├── Program.cs
├── Entities/
│   ├── OrderCreatedEvent.cs
│   ├── PaymentProcessedEvent.cs
│   └── ApplicationDbContext.cs
├── OutboxService/
│   ├── IOutboxPublisher.cs
│   ├── OutboxPublisher.cs
│   ├── OrderService.cs
│   └── OutboxPublisherService.cs
└── README.md
```

## How to run

```bash
dotnet run --project samples/outbox-persistence
```

## What is intentionally omitted

- Real outbox database table (using in-memory for demo)
- Message broker (RabbitMQ, Azure Service Bus, Kafka)
- Idempotent consumer implementation
- Dead-letter queue
- Retries with exponential backoff
- Integration with monitoring (OpenTelemetry)

## Related documents

- **ADR-0026**: Outbox pattern for reliable event publishing
- **ADR-0025**: API versioning strategy
- **ADR-0027**: Observability with OpenTelemetry
''')

    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    public OutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisherService starting (interval: {Interval}s)", _interval.TotalSeconds);

        var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishPendingEventsAsync(stoppingToken);
        }
    }

    private async Task PublishPendingEventsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Simulate: iterate over all unpublished outbox events
        // In a real application, you would use a properly configured outbox table
        var events = await context.UseInMemoryDatabase("outbox_demo").Set<object>().FromSqlRaw("SELECT 1").ToListAsync(); // placeholder

        // In production, this queries the OutboxMessages and OutboxEvents tables
        // GetPendingBatchesAsync() returns outbox rows that need publishing
        // Here we just log for demo purposes

        _logger.LogInformation("OutboxPublisherService: scanned outbox for pending events");
    }
}

{
    /// <summary>
    /// Publish an event, persisting it in the outbox table as part of the same transaction.
    /// </summary>
    Task PublishEventAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class;
}
''')

with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/OutboxService/OutboxPublisher.cs', 'w') as f:
    f.write('''// ADR-0026 — Outbox Publisher Implementation

using Microsoft.EntityFrameworkCore;
using OutboxPersistence.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OutboxPersistence.OutboxService;

public class OrderCreatedEventOutboxPublisher : IOutboxPublisher
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderCreatedEventOutboxPublisher> _logger;

    public OrderCreatedEventOutboxPublisher(
        ApplicationDbContext context,
        ILogger<OrderCreatedEventOutboxPublisher> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishEventAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        // Persist the event in the same transaction as the domain state change.
        // This guarantees atomicity: the domain change and the event are always in sync.
        _context.Add(@event);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Published event {@Event} to outbox", @event);
    }
}
''')

with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/OutboxService/OrderService.cs', 'w') as f:
    f.write('''// ADR-0026 — Order Service (domain service)

using OutboxPersistence.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OutboxPersistence.OutboxService;

public class OrderService : IOrderService
{
    private readonly IOutboxPublisher _publisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOutboxPublisher publisher, ILogger<OrderService> logger)
    {
        _publisher = publisher ?? throw new System.ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    public async Task PlaceOrderAsync(string tenantId, decimal amount, CancellationToken ct = default)
    {
        _logger.LogInformation("Placing order for tenant {TenantId} amount {Amount}", tenantId, amount);

        // 1. Persist the domain change (e.g., save order to DB)
        // Here we simulate the domain change with a log

        // 2. Publish event to outbox (atomic with the domain change)
        await _publisher.PublishEventAsync(new OrderCreatedEvent
        {
            OrderId = System.Guid.NewGuid(),
            TenantId = tenantId,
            Amount = amount,
            Status = "Placed"
        }, ct);

        _logger.LogInformation("Order placed for tenant {TenantId} - event published to outbox", tenantId);
    }
}
''')

print("OutboxService done")

namespace OutboxPersistence.Entities;

/// <summary>
/// Integration event for payment processing.
/// </summary>
public class PaymentProcessedEvent
{
    public Guid OrderId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}
''')

with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/Entities/ApplicationDbContext.cs', 'w') as f:
    f.write('''// ADR-0026 — EF Core DbContext

using Microsoft.EntityFrameworkCore;

namespace OutboxPersistence.Entities;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<OrderCreatedEvent> OrderCreatedEvents => Set(typeof(OrderCreatedEvent).Name!) as DbSet<OrderCreatedEvent>;
    public DbSet<PaymentProcessedEvent> PaymentProcessedEvents => Set(typeof(PaymentProcessedEvent).Name!) as DbSet<PaymentProcessedEvent>;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OrderCreatedEvent>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(100);
        });
        modelBuilder.Entity<PaymentProcessedEvent>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(100);
        });
    }
}
''')

print("Entities done")


  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="StackExchange.Redis" Version="2.8.16" />
  </ItemGroup>

</Project>
''')

with open('/Development/DotNet/Usman Architecture/samples/outbox-persistence/Program.cs', 'w') as f:
    f.write('''// ADR-0026 — Transactional Outbox Pattern

using Microsoft.EntityFrameworkCore;
using OutboxPersistence.OutboxService;
using OutboxPersistence.IntegrationEvents;
using OutboxPersistence.Entities;

var builder = WebApplication.CreateBuilder(args);

// --- EF Core / SQL Server ---
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=localhost;Database=outbox_demo;Trusted_Connection=True;TrustServerVersion=160;"));

// --- Outbox Publisher (runs as a hosted service) ---
builder.Services.AddHostedService<OutboxPublisherService>();

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    if (!context.Events.Any())
    {
        context.Events.AddRange(new[]
        {
            new OrderCreatedEvent { OrderId = Guid.NewGuid(), TenantId = "tenant_A", Amount = 100, Status = "Pending" },
            new PaymentProcessedEvent { OrderId = Guid.NewGuid(), TenantId = "tenant_A", Amount = 100, PaymentMethod = "credit_card" }
        });
        context.SaveChanges();
    }
}

var summary = app.Services.CreateScope().ServiceProvider.GetRequiredService<IOrderService>();

// Simulate placing an order
Console.WriteLine("=== Outbox Pattern Demo ===");
Console.WriteLine();

summary.PlaceOrderAsync("tenant_A", 250.00m).GetAwaiter().GetResult();

Console.WriteLine();
Console.WriteLine("Order placed. Outbox events are stored in the same transaction.");
Console.WriteLine("Press Ctrl+C to exit.");
Console.WriteLine();

Task.Delay(Timeout.Infinite).Wait();
''')
