namespace Scenarios.Payments;

public class PayPalStrategy : IPaymentStrategy
{
    public string ProviderName => "PayPal";

    public Task<PaymentResult> ProcessAsync(string customerId, decimal amount, string currency)
    {
        var txn = $"PP-{Guid.NewGuid():N}"[..16].ToUpper();
        Console.WriteLine($"[PayPal] Charged {amount:C} {currency} for {customerId} -> {txn}");
        return Task.FromResult(new PaymentResult(true, txn, ProviderName));
    }
}
