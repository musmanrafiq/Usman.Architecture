namespace Behavioral.Observer;

public class TriggerFraudCheckHandler : IOrderEventHandler
{
    public Task HandleAsync(OrderPlacedEvent evt)
    {
        Console.WriteLine($"[Fraud] Checking order {evt.OrderId} (total: {evt.Total:C})...");
        return Task.CompletedTask;
    }
}
