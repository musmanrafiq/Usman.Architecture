namespace Structural.Decorator;

public static class DecoratorDemo
{
    public static async Task RunAsync()
    {
        // Stacks: Metrics -> Logging -> OrderService (outermost runs first)
        IOrderService service =
            new MetricsOrderService(
                new LoggingOrderService(
                    new OrderService()));

        var id = await service.PlaceOrderAsync("PROD-777", 2);
        Console.WriteLine($"Final Order ID: {id}");
    }
}
