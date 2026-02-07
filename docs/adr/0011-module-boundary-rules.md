# ADR-0011: Module boundary rules

- Status: Accepted
- Date: 2026-02-07
- By: Usman Rafiq
- Related: ADR-0010

## Context

A “modular monolith” only works if modules stay modular.

Without explicit boundary rules, a single deployable quickly becomes a traditional monolith:

- modules reach into each other’s internals
- shared abstractions become dumping grounds
- the codebase becomes harder to change safely
- ownership becomes unclear

This repo needs rules that:

- are easy to understand
- are hard to “accidentally break”
- can be enforced with lightweight automation (architecture tests)

## Decision

Modules are treated as **internal products** with clear boundaries.

### Rule 1: No direct access to another module’s internals

A module must not reference:

- another module’s Domain layer
- another module’s Persistence/Infrastructure
- another module’s internal application services
- another module’s internal models/entities

**Allowed:** referencing another module only through its **published contracts**.

### Rule 2: Each module has a “public surface” (contracts)

If a module wants other modules to use something, it must expose it via a small, explicit surface such as:

- `Contracts` (DTOs, events, interfaces)
- a `PublicApi` namespace/folder
- integration event definitions

Everything else is private.

### Rule 3: Dependencies are one-way and explicit

- If Module A depends on Module B, it should be obvious from references.
- Hidden dependencies (shared helpers, shared DbContext, “common services”) are discouraged.
- Shared code is allowed only when it is truly cross-cutting and stable (logging, primitives, small shared kernel types).

### Rule 4: Data is owned by the module

A module owns its data and how it is accessed.

Other modules should not:

- query its tables directly
- use its DbContext
- reuse its repositories

(How this is implemented is defined in a later ADR on persistence.)

### Rule 5: Cross-module communication must not leak internals

Cross-module calls must not require:

- referencing another module’s Domain types
- referencing another module’s EF entities
- knowing internal implementation details

Allowed communication styles (detailed later):

- calling an interface defined in the target module’s contracts
- publishing/handling domain or integration events
- minimal application service contracts

### Rule 6: Boundaries must be enforceable

These rules are not just “guidelines”.

They must be enforced through:

- project/reference layout
- architecture tests (build-time or test-time)

If enforcement is hard, the rule is probably too complicated.

## Alternatives considered

### Option 1: “Boundaries by convention only”

**Pros**

- fast to start
- fewer rules

**Cons**

- boundaries erode silently over time
- violations happen under pressure
- the codebase becomes a traditional monolith again

Rejected.

### Option 2: Hard isolation with separate deployments (microservices)

**Pros**

- boundaries enforced by network and deployments

**Cons**

- distributed complexity too early
- higher operational overhead
- slower iteration for most teams initially

Rejected as a default. (Still a possible evolution later.)

### Option 3: Explicit boundaries + enforced rules (chosen)

**Pros**

- keeps deployment simple
- makes boundaries real
- supports gradual evolution
- aligns with ADR-0010

Chosen.

## Consequences

### Positive

- Clear ownership per module.
- Less accidental coupling.
- Easier refactoring and evolution over time.
- Enables future extraction to microservices if needed.

### Trade-offs

- Requires discipline and review culture.
- Sometimes feels slower than “just calling the other module”.
- Requires a small amount of tooling (architecture tests).

## Follow-up: Runnable proof required

This ADR requires a runnable sample to prove boundaries can be enforced.

A first sample should demonstrate:

- two or three modules
- a forbidden dependency example
- an architecture test that fails when a boundary is violated
- a README that links back to this ADR

Suggested location:
samples/module-boundaries/

## Notes

If boundaries are not enforceable, modular monolith becomes a naming convention.
This ADR exists to prevent that outcome.
