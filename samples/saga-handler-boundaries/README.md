# ADR-0030: Saga Handler Single-Purpose Boundaries

This sample demonstrates the **single-purpose handler pattern** for saga handlers:

- **Read handlers**: only SELECT queries, no writes or HTTP calls
- **Write handlers**: DB writes + outbox logging, no HTTP calls
- **HTTP handlers**: only external calls, no DB writes

## What this sample shows

### 1. Bad pattern (do everything in one handler)

```csharp
// BAD: one handler does everything
public async Task Handle(OrderPlacedEvent evt)
{
    var cart = await _db.Carts.FindAsync(evt.CartId);   // READ
    var total = CalculateTotal(cart);                    // BIZ LOGIC
    await _db.Orders.AddAsync(new Order {...});         // WRITE
    await _httpClient.PostAsJsonAsync("/payments", ...); // HTTP
}
// Problems:
// - Connection pool exhausted (DB connection held during HTTP call)
// - Long-running transaction (locks held for long time)
// - Hard to test (mixes concerns)
```

### 2. Good pattern (split into single-purpose handlers)

```csharp
// 1. Read handler
public Task Handle(OrderPlacedEvent evt) {
    var product = await _db.Products.FindAsync(evt.ProductId);
    // No writes, no HTTP
}

// 2. Write handler
public Task Handle(OrderPlacedEvent evt) {
    await _db.Orders.AddAsync(new Order {...});
    await _db.SaveChangesAsync(ct);
    await _outbox.PublishAsync(new OrderPlacedLoggedEvent {...});
}

// 3. HTTP handler
public Task Handle(OrderPlacedEvent evt) {
    var payment = await _httpClient.PostAsJsonAsync("/payments", evt.Amount);
    // No DB
}
```
{
    private readonly ConcurrentQueue<OutboxEntry> _queue = new();

    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        _queue.Enqueue(new OutboxEntry
        {
            EventType = @event.GetType().Name,
            Payload = @event
        });

        return Task.CompletedTask;
    }
}

public record OutboxEntry(string EventType, object Payload);
