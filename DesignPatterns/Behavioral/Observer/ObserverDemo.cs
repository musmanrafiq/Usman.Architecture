namespace Behavioral.Observer;

public static class ObserverDemo
{
    public static async Task RunAsync()
    {
        var dispatcher = new OrderEventDispatcher();
        dispatcher.Subscribe(new SendConfirmationEmailHandler());
        dispatcher.Subscribe(new UpdateLoyaltyPointsHandler());
        dispatcher.Subscribe(new TriggerFraudCheckHandler());

        await dispatcher.PublishAsync(new OrderPlacedEvent("ORD-001", "CUST-42", 149.99m));
    }
}
