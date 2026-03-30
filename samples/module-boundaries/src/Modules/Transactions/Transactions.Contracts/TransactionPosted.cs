namespace Transactions.Contracts;

/// <summary>
/// Integration event published by the Transactions module.
/// </summary>
public sealed record TransactionPosted(Guid TransactionId, Guid AccountId, decimal Amount);
