namespace Structural.Adapter;

/// <summary>
/// Adapts the incompatible LegacyPayPalSdk to the IPaymentGateway interface
/// without modifying the legacy code.
/// </summary>
public class PayPalAdapter(LegacyPayPalSdk sdk, string payPalEmail) : IPaymentGateway
{
    public bool Charge(string customerId, decimal amount, string currency)
    {
        if (currency != "USD")
        {
            Console.WriteLine($"[PayPalAdapter] Currency {currency} not supported, skipping.");
            return false;
        }

        var txn = sdk.ExecuteTransaction(payPalEmail, (double)amount);
        Console.WriteLine($"[PayPalAdapter] Transaction ID: {txn}");
        return txn.Length > 0;
    }
}
