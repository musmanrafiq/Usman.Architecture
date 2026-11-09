namespace Behavioral.Observer;

public interface IOrderEventHandler
{
    Task HandleAsync(OrderPlacedEvent evt);
}
