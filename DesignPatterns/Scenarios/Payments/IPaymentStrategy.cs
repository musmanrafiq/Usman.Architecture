namespace Scenarios.Payments;

public interface IPaymentStrategy
{
    string ProviderName { get; }
    Task<PaymentResult> ProcessAsync(string customerId, decimal amount, string currency);
}
