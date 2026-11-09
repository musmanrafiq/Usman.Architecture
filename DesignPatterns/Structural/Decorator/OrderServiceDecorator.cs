namespace Structural.Decorator;

/// <summary>
/// Base decorator — delegates to the wrapped IOrderService by default.
/// Subclasses override only the behaviour they want to add.
/// </summary>
public abstract class OrderServiceDecorator(IOrderService inner) : IOrderService
{
    public virtual Task<string> PlaceOrderAsync(string productId, int quantity)
        => inner.PlaceOrderAsync(productId, quantity);
}
