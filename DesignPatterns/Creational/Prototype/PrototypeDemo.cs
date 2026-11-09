namespace Creational.Prototype;

public static class PrototypeDemo
{
    public static void Run()
    {
        var invoiceProto = new InvoiceTemplate
        {
            Title = "Standard Invoice",
            Content = "Invoice template content...",
            Tags = ["finance", "billing"],
            CurrencyCode = "GBP"
        };

        var cloned = (InvoiceTemplate)invoiceProto.Clone();
        cloned.Title = "Invoice #1042";
        cloned.Tags.Add("Q1-2026");

        Console.WriteLine($"Original: {invoiceProto}");
        Console.WriteLine($"Clone:    {cloned}");
        Console.WriteLine($"Tags are independent: {!ReferenceEquals(invoiceProto.Tags, cloned.Tags)}");
    }
}
