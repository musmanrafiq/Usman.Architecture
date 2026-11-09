namespace Structural.Facade;

public class ShippingService
{
    public string CreateShipment(string orderId, string address)
    {
        var trackingId = $"SHIP-{orderId[..4]}-{Guid.NewGuid():N}"[..16];
        Console.WriteLine($"[Shipping] Shipment {trackingId} created for {address}");
        return trackingId;
    }
}
