namespace Creational.Prototype;

public class InvoiceTemplate : DocumentTemplate
{
    public string CurrencyCode { get; set; } = "USD";

    public override DocumentTemplate Clone() =>
        new InvoiceTemplate
        {
            Title = Title,
            Content = Content,
            Tags = [.. Tags],   // deep copy — independent list
            CurrencyCode = CurrencyCode
        };
}
