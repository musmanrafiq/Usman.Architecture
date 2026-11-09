namespace Behavioral.Observer;

public class OrderEventDispatcher
{
    private readonly List<IOrderEventHandler> _handlers = [];

    public void Subscribe(IOrderEventHandler handler) => _handlers.Add(handler);
    public void Unsubscribe(IOrderEventHandler handler) => _handlers.Remove(handler);

    public async Task PublishAsync(OrderPlacedEvent evt)
    {
        Console.WriteLine($"[Dispatcher] Publishing OrderPlacedEvent for order {evt.OrderId}");
        foreach (var handler in _handlers)
            await handler.HandleAsync(evt);
    }
}
