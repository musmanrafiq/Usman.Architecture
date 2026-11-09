// ADR-0026 — Transactional Outbox Pattern

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
