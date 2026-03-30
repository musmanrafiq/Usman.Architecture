namespace Structural.Decorator;

public class MetricsOrderService(IOrderService inner) : OrderServiceDecorator(inner)
{
    public override async Task<string> PlaceOrderAsync(string productId, int quantity)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var orderId = await base.PlaceOrderAsync(productId, quantity);
        sw.Stop();
        Console.WriteLine($"[METRICS] PlaceOrder took {sw.ElapsedMilliseconds}ms");
        return orderId;
    }
}
