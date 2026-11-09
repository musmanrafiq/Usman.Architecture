namespace Scenarios.Payments;

public class StripeStrategy : IPaymentStrategy
{
    public string ProviderName => "Stripe";

    public Task<PaymentResult> ProcessAsync(string customerId, decimal amount, string currency)
    {
        var txn = $"ch_{Guid.NewGuid():N}"[..16];
        Console.WriteLine($"[Stripe] Charged {amount:C} {currency} for {customerId} -> {txn}");
        return Task.FromResult(new PaymentResult(true, txn, ProviderName));
    }
}
