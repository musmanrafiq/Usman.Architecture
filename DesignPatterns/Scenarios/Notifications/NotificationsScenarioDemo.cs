namespace Scenarios.Notifications;

public static class NotificationsScenarioDemo
{
    public static async Task RunAsync()
    {
        var handler = new NotificationHandler([
            new EmailChannel(),
            new SmsChannel(),
            new PushChannel()
        ]);

        var dispatcher = new NotificationDispatcher();
        dispatcher.Register(handler);

        await dispatcher.DispatchAsync(new NotificationEvent(
            "CUST-01", "Order Shipped",
            "Your order ORD-999 is on its way!", NotificationPriority.Normal));

        await dispatcher.DispatchAsync(new NotificationEvent(
            "CUST-02", "Account Locked",
            "Suspicious login detected. Reset your password.", NotificationPriority.Critical));
    }
}
