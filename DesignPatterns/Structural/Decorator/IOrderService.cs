namespace Structural.Decorator;

public interface IOrderService
{
    Task<string> PlaceOrderAsync(string productId, int quantity);
}
