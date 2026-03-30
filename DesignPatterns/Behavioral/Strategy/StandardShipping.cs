namespace Behavioral.Strategy;

public class StandardShipping : IShippingStrategy
{
    public string ProviderName => "Standard (5-7 days)";

    public decimal CalculateCost(decimal weightKg, string destination)
        => Math.Round(2.50m + weightKg * 0.80m, 2);
}
