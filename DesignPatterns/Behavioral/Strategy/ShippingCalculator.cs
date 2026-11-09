namespace Behavioral.Strategy;

public class ShippingCalculator(IShippingStrategy strategy)
{
    private IShippingStrategy _strategy = strategy;

    /// <summary>Strategies can be swapped at runtime without changing the caller.</summary>
    public void SetStrategy(IShippingStrategy strategy) => _strategy = strategy;

    public decimal Calculate(decimal weightKg, string destination)
    {
        var cost = _strategy.CalculateCost(weightKg, destination);
        Console.WriteLine($"[{_strategy.ProviderName}] {weightKg}kg to '{destination}' -> {cost:C}");
        return cost;
    }
}
