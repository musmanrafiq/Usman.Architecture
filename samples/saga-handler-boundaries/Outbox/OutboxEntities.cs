// ADR-0030 -- Outbox event entities for demo
using System;

namespace SagaHandlerBoundaries.Outbox;

public class OrderPlacedLoggedEvent
{
    public Guid OrderId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
    private readonly IOutboxPublisher _outbox;
    private readonly ILogger<ChargePaymentHandler> _logger;

    public ChargePaymentHandler(IHttpClientFactory httpFactory, IOutboxPublisher outbox, ILogger<ChargePaymentHandler> logger)
    {
        _httpFactory = httpFactory;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(OrderPlacedEvent evt, CancellationToken ct)
    {
        // HTTP ONLY: no DB operations
        var client = _httpFactory.CreateClient("paymentGateway");
        var payment = await client.PostAsJsonAsync("/payments", new { amount = evt.Amount, currency = "USD" }, ct);

        if (payment.IsSuccessStatusCode)
        {
            _logger.LogInformation("HTTP: payment {Amount} processed for order {OrderId}", evt.Amount, evt.OrderId);
        }
        else
        {
            // Publish failure to outbox (ADR-0026) for retry
            await _outbox.PublishAsync(new PaymentFailedEvent
            {
                OrderId = evt.OrderId,
                TenantId = evt.TenantId,
                Amount = evt.Amount
            }, ct);

            _logger.LogWarning("HTTP: payment failed for order {OrderId}", evt.OrderId);
        }
    }
}
public class PublishToInventoryHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly AppDbContext _db;
    private readonly IOutboxPublisher _outbox;
    private readonly ILogger<PublishToInventoryHandler> _logger;

    public PublishToInventoryHandler(AppDbContext db, IOutboxPublisher outbox, ILogger<PublishToInventoryHandler> logger)
    {
        _db = db;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(OrderPlacedEvent evt, CancellationToken ct)
    {
        // WRITE: DB transaction (short-lived, no external calls)
        await _db.Orders.AddAsync(new Order
        {
            OrderId = evt.OrderId,
            TenantId = evt.TenantId,
            Amount = evt.Amount,
            Status = "Placed"
        });
        await _db.SaveChangesAsync(ct);

        // Log to outbox (ADR-0026)
        await _outbox.PublishAsync(new OrderPlacedLoggedEvent
        {
            OrderId = evt.OrderId,
            TenantId = evt.TenantId,
            Amount = evt.Amount
        }, ct);

        _logger.LogInformation("Wrote: order {OrderId} placed", evt.OrderId);
    }
}

        _logger.LogInformation("Read: product {ProductId} for order {OrderId}",
            evt.ProductId, evt.OrderId);

        // Project to read model (out of scope for this demo)
    }
}
