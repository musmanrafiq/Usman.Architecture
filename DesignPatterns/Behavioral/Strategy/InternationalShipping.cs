namespace Behavioral.Strategy;

public class InternationalShipping : IShippingStrategy
{
    public string ProviderName => "International DHL";

    public decimal CalculateCost(decimal weightKg, string destination)
        => Math.Round(19.99m + weightKg * 3.00m, 2);
}
