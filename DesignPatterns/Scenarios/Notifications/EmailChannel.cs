namespace Scenarios.Notifications;

public class EmailChannel : IDeliveryChannel
{
    public bool CanHandle(NotificationPriority priority) => true; // handles all priorities

    public Task SendAsync(string userId, string subject, string body)
    {
        Console.WriteLine($"[Email] -> {userId} | Subject: {subject}");
        return Task.CompletedTask;
    }
}
