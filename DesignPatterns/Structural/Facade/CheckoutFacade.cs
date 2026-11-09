namespace Structural.Facade;

/// <summary>
/// Single entry point that orchestrates Inventory, Payment, Shipping and Notifications.
/// Callers interact only with this class — they have no knowledge of the subsystems.
/// </summary>
public class CheckoutFacade(
    InventoryService inventory,
    PaymentService payment,
    ShippingService shipping,
    NotificationService notifications)
{
    public string PlaceOrder(string customerId, string productId, int qty, decimal price, string address)
    {
        if (!inventory.Reserve(productId, qty))
            throw new InvalidOperationException("Out of stock.");

        if (!payment.Charge(customerId, price * qty))
            throw new InvalidOperationException("Payment failed.");

        var orderId = $"ORD-{Guid.NewGuid():N}"[..12].ToUpper();
        var trackingId = shipping.CreateShipment(orderId, address);
        notifications.Notify(customerId, $"Your order {orderId} is on its way! Tracking: {trackingId}");

        return orderId;
    }
}
