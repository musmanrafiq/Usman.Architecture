# ADR-0010: Modular monolith as the default starting architecture

- Status: In-Progress
- Date: 2026-02-06
- By: Usman Rafiq
- Related: ADR-0001, ADR-0002

## Context

In many teams, architectural discussions start with a false choice:

> “Should we build a monolith or microservices?”

In practice, the real problem is not scale — it is **change**.

Most systems start small, evolve unpredictably, and are maintained by:

- small to medium-sized teams
- developers with mixed experience levels
- limited operational bandwidth

Microservices are often introduced early to “future-proof” the system, but this usually comes with immediate costs:

- distributed complexity
- operational overhead
- debugging and observability challenges
- premature infrastructure decisions

At the same time, traditional monoliths often degrade into:

- tightly coupled codebases
- unclear ownership
- accidental shared state
- fear-driven development

The challenge is to find an approach that:

- supports change
- enforces boundaries
- keeps operational complexity low
- allows evolution without rewrites

## Decision

The default architectural starting point is a **modular monolith**.

A modular monolith is:

- a single deployable unit
- with explicitly defined internal modules
- strong boundaries between modules
- clear ownership of data and behavior

This choice prioritizes:

- clear boundaries over physical distribution
- simplicity of deployment
- architectural discipline early, without distributed systems overhead

Microservices are treated as a **possible future evolution**, not a starting assumption.

## What this means in practice

A modular monolith:

- runs as a single application process
- enforces module boundaries through code structure and rules
- avoids shared “everything” abstractions
- makes dependencies explicit and intentional

Modules:

- own their domain logic
- control access to their data
- communicate through well-defined interfaces or events

The goal is not to delay microservices forever, but to **earn them**.

## Alternatives considered

### Option 1: Start with microservices

**Why it was considered**

- independent deployments
- team autonomy
- perceived scalability benefits

**Why it was rejected**

- introduces distributed complexity before it is needed
- increases cognitive and operational load
- makes early-stage development slower
- often solves problems that do not yet exist

---

### Option 2: Start with a traditional monolith

**Why it was considered**

- simple to build
- fast initial development
- minimal infrastructure

**Why it was rejected**

- weak or implicit boundaries
- high risk of coupling over time
- difficult to evolve safely as the system grows

---

### Option 3: Modular monolith (chosen)

**Why it was chosen**

- keeps deployment simple
- enforces architectural boundaries early
- supports gradual evolution
- aligns with real-world constraints of most teams

## Consequences

### Positive

- Clear module boundaries from the start.
- Lower operational complexity than microservices.
- Easier debugging and local development.
- Better foundation for future architectural evolution.

### Trade-offs

- Requires discipline to maintain boundaries.
- Boundaries are enforced by convention and tooling, not network isolation.
- Does not provide independent deployment by default.

## When this decision might change

This decision may be revisited when:

- teams require independent deployments
- modules have significantly different scaling characteristics
- organizational structure demands strong isolation
- operational maturity supports distributed systems

Any change from a modular monolith to microservices should be:

- intentional
- justified by concrete constraints
- documented as a new ADR

## Notes

This ADR defines a starting point, not an end state.

The value of a modular monolith depends on how well boundaries are respected.
Subsequent ADRs will define:

- module boundary rules
- communication patterns
- data ownership and persistence strategies
