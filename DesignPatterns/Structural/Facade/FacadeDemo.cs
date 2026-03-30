namespace Structural.Facade;

public static class FacadeDemo
{
    public static void Run()
    {
        var facade = new CheckoutFacade(
            new InventoryService(),
            new PaymentService(),
            new ShippingService(),
            new NotificationService());

        var orderId = facade.PlaceOrder(
            customerId: "CUST-42",
            productId: "WIDGET-PRO",
            qty: 2,
            price: 19.99m,
            address: "123 Main St, London");

        Console.WriteLine($"Order created: {orderId}");
    }
}
