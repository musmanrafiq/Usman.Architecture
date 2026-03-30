namespace Scenarios.Payments;

/// <summary>Context — holds the active strategy and delegates payment to it.</summary>
public class PaymentProcessor
{
    private IPaymentStrategy _strategy;

    public PaymentProcessor(PaymentProvider provider)
        => _strategy = PaymentStrategyFactory.Create(provider);

    public void SwitchProvider(PaymentProvider provider)
        => _strategy = PaymentStrategyFactory.Create(provider);

    public Task<PaymentResult> PayAsync(string customerId, decimal amount, string currency = "GBP")
        => _strategy.ProcessAsync(customerId, amount, currency);
}
