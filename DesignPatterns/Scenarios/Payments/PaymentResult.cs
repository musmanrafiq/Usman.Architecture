namespace Scenarios.Payments;

public record PaymentResult(bool Success, string TransactionId, string Provider);
