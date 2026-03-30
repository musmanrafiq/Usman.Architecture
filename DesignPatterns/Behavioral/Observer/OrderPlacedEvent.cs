namespace Behavioral.Observer;

public record OrderPlacedEvent(string OrderId, string CustomerId, decimal Total);
