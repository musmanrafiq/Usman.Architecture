namespace Scenarios.Notifications;

public class SmsChannel : IDeliveryChannel
{
    public bool CanHandle(NotificationPriority priority)
        => priority >= NotificationPriority.High; // SMS only for urgent messages

    public Task SendAsync(string userId, string subject, string body)
    {
        Console.WriteLine($"[SMS]   -> {userId} | {subject}: {body[..Math.Min(body.Length, 40)]}...");
        return Task.CompletedTask;
    }
}
