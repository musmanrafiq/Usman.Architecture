namespace Scenarios.Notifications;

public class PushChannel : IDeliveryChannel
{
    public bool CanHandle(NotificationPriority priority)
        => priority >= NotificationPriority.Normal;

    public Task SendAsync(string userId, string subject, string body)
    {
        Console.WriteLine($"[Push]  -> {userId} | {subject}");
        return Task.CompletedTask;
    }
}
