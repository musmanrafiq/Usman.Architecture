namespace Scenarios.Notifications;

/// <summary>
/// Observer — subscribes to NotificationEvents and routes each one
/// to whichever delivery channels can handle the event's priority (Strategy).
/// </summary>
public class NotificationHandler(IEnumerable<IDeliveryChannel> channels)
{
    public async Task HandleAsync(NotificationEvent evt)
    {
        var eligible = channels.Where(c => c.CanHandle(evt.Priority));
        foreach (var channel in eligible)
            await channel.SendAsync(evt.UserId, evt.Subject, evt.Body);
    }
}
