# Module boundaries (2 modules + tests)

This sample demonstrates a pragmatic way to **enforce module boundaries** in a modular monolith (or a well-structured monorepo):

- Modules have **Domain** assemblies (implementation)
- Modules expose **Contracts** assemblies (events/DTOs)
- **Architecture tests** prevent accidental cross-module dependencies

## Structure

- `src/Modules/Accounts/Accounts.Domain`
- `src/Modules/Accounts/Accounts.Contracts`
- `src/Modules/Transactions/Transactions.Domain`
- `src/Modules/Transactions/Transactions.Contracts`
- `src/ArchitectureTests` (xUnit + NetArchTest)

## Boundary rules enforced

- `Accounts.Domain` must not depend on anything in `Transactions.*`
- `Transactions.Domain` must not depend on anything in `Accounts.*`
- `*.Contracts` must not depend on any `*.Domain`

In other words: if another module needs something, it should depend on the **Contracts** (public API), not the **Domain** (implementation).

## Run it

From `samples/module-boundaries/src`:

- `dotnet test`

## Try breaking the rule (expected failure)

1. Add a project reference from `Accounts.Domain` to `Transactions.Domain`.
2. Use any `Transactions.Domain` type inside `Accounts.Domain`.
3. Run tests again.

The `ArchitectureTests` project should fail and point at the offending types.
