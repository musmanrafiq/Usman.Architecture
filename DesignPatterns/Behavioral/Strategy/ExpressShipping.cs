namespace Behavioral.Strategy;

public class ExpressShipping : IShippingStrategy
{
    public string ProviderName => "Express (1-2 days)";

    public decimal CalculateCost(decimal weightKg, string destination)
        => Math.Round(9.99m + weightKg * 1.50m, 2);
}
