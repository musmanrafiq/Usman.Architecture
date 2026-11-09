namespace Scenarios.Payments;

/// <summary>
/// Factory — creates the correct IPaymentStrategy from a PaymentProvider enum value.
/// New providers require adding one case here — no other code changes.
/// </summary>
public static class PaymentStrategyFactory
{
    public static IPaymentStrategy Create(PaymentProvider provider) => provider switch
    {
        PaymentProvider.Stripe => new StripeStrategy(),
        PaymentProvider.PayPal => new PayPalStrategy(),
        PaymentProvider.BankTransfer => new BankTransferStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(provider))
    };
}
