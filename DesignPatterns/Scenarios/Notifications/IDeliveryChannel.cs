namespace Scenarios.Notifications;

public interface IDeliveryChannel
{
    bool CanHandle(NotificationPriority priority);
    Task SendAsync(string userId, string subject, string body);
}
