namespace Scenarios.Payments;

public static class PaymentsScenarioDemo
{
    public static async Task RunAsync()
    {
        var processor = new PaymentProcessor(PaymentProvider.Stripe);
        var r1 = await processor.PayAsync("CUST-001", 99.99m);

        processor.SwitchProvider(PaymentProvider.PayPal);
        var r2 = await processor.PayAsync("CUST-002", 149.00m);

        processor.SwitchProvider(PaymentProvider.BankTransfer);
        var r3 = await processor.PayAsync("CUST-003", 2500.00m);

        Console.WriteLine($"\nResults: {r1.TransactionId} | {r2.TransactionId} | {r3.TransactionId}");
    }
}
