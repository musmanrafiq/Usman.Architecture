using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests;

public sealed class ModuleBoundaryTests
{
    private static readonly Assembly AccountsDomainAssembly = typeof(Accounts.Domain.Account).Assembly;
    private static readonly Assembly AccountsContractsAssembly = typeof(Accounts.Contracts.AccountCreated).Assembly;
    private static readonly Assembly TransactionsDomainAssembly = typeof(Transactions.Domain.Transaction).Assembly;
    private static readonly Assembly TransactionsContractsAssembly = typeof(Transactions.Contracts.TransactionPosted).Assembly;

    [Fact]
    public void Accounts_domain_should_not_depend_on_transactions()
    {
        var result = Types
            .InAssembly(AccountsDomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Transactions.Domain",
                "Transactions.Contracts",
                "Transactions")
            .GetResult();

        var offenders = result.FailingTypeNames ?? Array.Empty<string>();

        Assert.True(
            result.IsSuccessful,
            $"Accounts.Domain depends on Transactions. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Transactions_domain_should_not_depend_on_accounts()
    {
        var result = Types
            .InAssembly(TransactionsDomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Accounts.Domain",
                "Accounts.Contracts",
                "Accounts")
            .GetResult();

        var offenders = result.FailingTypeNames ?? Array.Empty<string>();

        Assert.True(
            result.IsSuccessful,
            $"Transactions.Domain depends on Accounts. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Contracts_should_not_depend_on_any_domain()
    {
        var accountsContractsResult = Types
            .InAssembly(AccountsContractsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Accounts.Domain", "Transactions.Domain")
            .GetResult();

        var accountsContractsOffenders = accountsContractsResult.FailingTypeNames ?? Array.Empty<string>();

        Assert.True(
            accountsContractsResult.IsSuccessful,
            $"Accounts.Contracts depends on a Domain assembly. Offenders: {string.Join(", ", accountsContractsOffenders)}");

        var transactionsContractsResult = Types
            .InAssembly(TransactionsContractsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Accounts.Domain", "Transactions.Domain")
            .GetResult();

        var transactionsContractsOffenders = transactionsContractsResult.FailingTypeNames ?? Array.Empty<string>();

        Assert.True(
            transactionsContractsResult.IsSuccessful,
            $"Transactions.Contracts depends on a Domain assembly. Offenders: {string.Join(", ", transactionsContractsOffenders)}");
    }

    [Fact]
    public void Domains_should_not_reference_each_other_by_assembly_reference()
    {
        var accountsRefs = AccountsDomainAssembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.DoesNotContain("Transactions.Domain", accountsRefs);
        Assert.DoesNotContain("Transactions.Contracts", accountsRefs);

        var transactionsRefs = TransactionsDomainAssembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        Assert.DoesNotContain("Accounts.Domain", transactionsRefs);
        Assert.DoesNotContain("Accounts.Contracts", transactionsRefs);
    }
}
