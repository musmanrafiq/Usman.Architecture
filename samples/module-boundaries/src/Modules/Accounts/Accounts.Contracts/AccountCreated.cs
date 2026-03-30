namespace Accounts.Contracts;

/// <summary>
/// Integration event published by the Accounts module.
/// Other modules may reference this Contracts assembly, but should not reference Accounts.Domain.
/// </summary>
public sealed record AccountCreated(Guid AccountId, string OwnerName);
