namespace Accounts.Domain;

public sealed class Account
{
    public Account(Guid id, string ownerName)
    {
        if (id == Guid.Empty) throw new ArgumentException("Account id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(ownerName)) throw new ArgumentException("Owner name is required.", nameof(ownerName));

        Id = id;
        OwnerName = ownerName;
    }

    public Guid Id { get; }
    public string OwnerName { get; }
}
