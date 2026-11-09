// ADR-0030 — Saga handler single-purpose boundaries

using Microsoft.EntityFrameworkCore;
using SagaHandlerBoundaries.Handlers;
using SagaHandlerBoundaries.Outbox;

var builder = WebApplication.CreateBuilder(args);

// Use in-memory DB for demo
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseInMemoryDatabase("saga_demo"));

// Register handlers
builder.Services.AddScoped<IEventHandler<OrderPlacedEvent>, ListenForOrderPlacedHandler>();
builder.Services.AddScoped<IEventHandler<OrderPlacedEvent>, PublishToInventoryHandler>();
builder.Services.AddScoped<IEventHandler<OrderPlacedEvent>, ChargePaymentHandler>();
builder.Services.AddScoped<IOutboxPublisher, InMemoryOutboxPublisher>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.EnsureCreated();
}

var summary = app.Services.CreateScope().ServiceProvider.GetRequiredService<IEventHandler<OrderPlacedEvent>>();

Console.WriteLine("=== Saga Handler Boundaries Demo ===");
Console.WriteLine();

// Run all three handlers (simulating the saga flow)
await summary.Handle(new OrderPlacedEvent
{
    OrderId = Guid.NewGuid(),
    TenantId = "tenant_A",
    Amount = 250.00m,
    ProductId = "PROD-001",
    ProductName = "Premium Widget"
});

Console.WriteLine();
Console.WriteLine("All handlers executed. Demonstrates read/write/HTTP single-purpose boundaries.");
Console.WriteLine("Press Ctrl+C to exit.");
Console.WriteLine();

Task.Delay(Timeout.Infinite).Wait();
