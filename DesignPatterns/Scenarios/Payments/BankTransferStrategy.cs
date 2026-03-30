namespace Scenarios.Payments;

public class BankTransferStrategy : IPaymentStrategy
{
    public string ProviderName => "BankTransfer";

    public Task<PaymentResult> ProcessAsync(string customerId, decimal amount, string currency)
    {
        var txn = $"BT-{DateTime.UtcNow:yyyyMMddHHmmss}-{customerId[..4]}";
        Console.WriteLine($"[Bank] Transfer of {amount:C} {currency} initiated -> {txn}");
        return Task.FromResult(new PaymentResult(true, txn, ProviderName));
    }
}
