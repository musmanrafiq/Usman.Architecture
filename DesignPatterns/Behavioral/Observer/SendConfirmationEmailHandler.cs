namespace Behavioral.Observer;

public class SendConfirmationEmailHandler : IOrderEventHandler
{
    public Task HandleAsync(OrderPlacedEvent evt)
    {
        Console.WriteLine($"[Email] Confirmation sent to customer {evt.CustomerId} for order {evt.OrderId}");
        return Task.CompletedTask;
    }
}
