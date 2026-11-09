namespace Behavioral.Strategy;

public interface IShippingStrategy
{
    string ProviderName { get; }
    decimal CalculateCost(decimal weightKg, string destination);
}
