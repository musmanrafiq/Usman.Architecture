// ADR-0030 -- Event definition
using System;

namespace SagaHandlerBoundaries.Entities;

public class OrderPlacedEvent
{
    public Guid OrderId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}
