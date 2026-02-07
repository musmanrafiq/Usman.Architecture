# ADR-0002: How this repository is structured

- Status: In-Progress
- Date: 2026-02-06
- By: Usman Rafiq
- Related: ADR-0001

## Context

This repository is intentionally not a traditional codebase.

It exists to capture:

- architectural reasoning
- concrete decisions
- small, focused proofs

Without clear structure and explicit rules, it would quickly drift into:

- scattered notes
- tool-driven documentation
- oversized demo applications

This ADR defines how the repository is organized, why it is organized this way, and what is intentionally out of scope.

## Decision

The repository is structured around **intent**, not technology.

Each type of content has a single responsibility:

- **Topics** explain architectural concepts and trade-offs.
- **Architecture Decision Records (ADRs)** capture concrete decisions at a point in time.
- **Samples** exist only to validate or challenge a specific decision.

Content is added only when it serves one of these purposes.

### High-level structure

docs/
├── 00-Start-Here/ # How to read and navigate this repository
├── topics/ # Architectural concepts and trade-offs
└── adr/ # Architecture Decision Records

samples/
└── <decision-or-topic>/ # Runnable proofs tied to one idea

Folders are created only when they contain content.
Empty or speculative folders are avoided.

---

## Topics

**Purpose**

Topics describe _why_ and _when_ an architectural approach makes sense.

They focus on:

- context
- constraints
- trade-offs
- boundaries

Topics are not tutorials and do not aim to be exhaustive references.

**Rules**

- Topics must be understandable without reading samples.
- Topics may reference ADRs for concrete decisions.
- Topics should avoid large code listings.
- Topics evolve over time as understanding improves.

---

## Architecture Decision Records (ADRs)

**Purpose**

ADRs capture **decisions**, not general explanations.

An ADR answers:

- What decision was made?
- In what context?
- What alternatives were considered?
- What are the consequences?

**Rules**

- One ADR documents one decision.
- ADRs must remain short and scannable.
- ADRs may link to:
  - related topics
  - other ADRs
  - samples (when proof is required)

ADRs may change status:

- Proposed
- Accepted
- Superseded
- Rejected

Changes in direction are recorded, not erased.

---

## Samples

**Purpose**

Samples exist to **prove or disprove a specific architectural idea**.

They are intentionally small and focused.

A sample may demonstrate:

- a boundary rule
- an enforcement mechanism
- a trade-off in practice
- a failure mode

**Rules**

- A sample must support one ADR or one clearly defined rule.
- A sample must be runnable with minimal setup.
- Each sample must include a README explaining:
  - what the sample demonstrates
  - which ADR(s) it supports
  - what is intentionally omitted

**Non-goals**

Samples must not:

- grow into feature-complete applications
- showcase UI, styling, or frameworks
- attempt to demonstrate multiple architectural ideas at once

---

## Relationships between artifacts

The expected flow of information is:

Topic → ADR → Sample

- Topics provide context.
- ADRs record decisions.
- Samples validate decisions.

Reverse references are allowed for navigation:

- ADRs may link back to Topics.
- Samples should link back to the ADR they support.

---

## What is intentionally out of scope

This repository does not contain:

- application templates or starters
- reusable frameworks
- full production reference systems
- exhaustive tool documentation

Such artifacts may exist in separate repositories and be referenced when useful.

---

## Consequences

### Positive

- The repository remains readable and long-lived.
- Architectural reasoning stays visible.
- Samples remain small and intentional.
- Scope creep is actively discouraged.

### Trade-offs

- Progress may feel slower than building large demo apps.
- Some readers will expect more code than is provided.
- Maintaining discipline requires ongoing effort.

---

## Notes

If this structure proves limiting or ineffective, it can be revised.
Any significant revision must be documented in a new ADR.
