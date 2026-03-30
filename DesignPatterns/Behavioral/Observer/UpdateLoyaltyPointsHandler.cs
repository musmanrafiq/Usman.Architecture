namespace Behavioral.Observer;

public class UpdateLoyaltyPointsHandler : IOrderEventHandler
{
    public Task HandleAsync(OrderPlacedEvent evt)
    {
        var points = (int)(evt.Total * 10);
        Console.WriteLine($"[Loyalty] +{points} points added for customer {evt.CustomerId}");
        return Task.CompletedTask;
    }
}
