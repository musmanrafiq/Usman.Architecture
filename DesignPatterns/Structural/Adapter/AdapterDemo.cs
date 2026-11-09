namespace Structural.Adapter;

public static class AdapterDemo
{
    public static void Run()
    {
        IPaymentGateway gateway = new PayPalAdapter(new LegacyPayPalSdk(), "merchant@example.com");
        var success = gateway.Charge("cust-001", 49.99m, "USD");
        Console.WriteLine($"Charge succeeded: {success}");
    }
}
