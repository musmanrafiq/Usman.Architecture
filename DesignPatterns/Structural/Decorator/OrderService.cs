namespace Structural.Decorator;

public class OrderService : IOrderService
{
    public Task<string> PlaceOrderAsync(string productId, int quantity)
    {
        var orderId = $"ORD-{Guid.NewGuid():N}"[..12];
        return Task.FromResult(orderId);
    }
}
