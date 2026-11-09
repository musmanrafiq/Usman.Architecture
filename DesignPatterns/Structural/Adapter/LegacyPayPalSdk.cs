namespace Structural.Adapter;

/// <summary>Legacy third-party SDK — we cannot modify this class.</summary>
public class LegacyPayPalSdk
{
    public string ExecuteTransaction(string payPalEmail, double usdAmount)
    {
        Console.WriteLine($"[PayPal SDK] Charging ${usdAmount} for {payPalEmail}");
        return "PP-TXN-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}
