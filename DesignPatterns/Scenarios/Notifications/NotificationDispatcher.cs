namespace Scenarios.Notifications;

/// <summary>Subject — publishes events to all registered handlers.</summary>
public class NotificationDispatcher
{
    private readonly List<NotificationHandler> _handlers = [];

    public void Register(NotificationHandler handler) => _handlers.Add(handler);

    public async Task DispatchAsync(NotificationEvent evt)
    {
        Console.WriteLine($"\n[Dispatcher] Sending '{evt.Subject}' [{evt.Priority}] to user {evt.UserId}");
        foreach (var handler in _handlers)
            await handler.HandleAsync(evt);
    }
}
