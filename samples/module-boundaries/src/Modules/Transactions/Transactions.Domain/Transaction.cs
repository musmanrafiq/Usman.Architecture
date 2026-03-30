namespace Transactions.Domain;

public sealed class Transaction
{
    public Transaction(Guid id, Guid accountId, decimal amount)
    {
        if (id == Guid.Empty) throw new ArgumentException("Transaction id cannot be empty.", nameof(id));
        if (accountId == Guid.Empty) throw new ArgumentException("Account id cannot be empty.", nameof(accountId));
        if (amount == 0) throw new ArgumentException("Amount must be non-zero.", nameof(amount));

        Id = id;
        AccountId = accountId;
        Amount = amount;
    }

    public Guid Id { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
}
