namespace Structural.Decorator;

public class LoggingOrderService(IOrderService inner) : OrderServiceDecorator(inner)
{
    public override async Task<string> PlaceOrderAsync(string productId, int quantity)
    {
        Console.WriteLine($"[LOG] Placing order: product={productId}, qty={quantity}");
        var orderId = await base.PlaceOrderAsync(productId, quantity);
        Console.WriteLine($"[LOG] Order placed: {orderId}");
        return orderId;
    }
}
