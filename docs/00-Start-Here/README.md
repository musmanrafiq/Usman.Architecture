# Start Here

This repository is an architecture notebook.

It captures how architectural decisions are made, why certain choices were taken, and what trade-offs were considered — based on real .NET systems and real constraints.

If you are new here, start with this page.

---

## How to read this repository

This repository is organized around **intent**, not tools.

There are three primary types of content:

### 1. Topics

Topics explain architectural concepts and trade-offs.

They focus on:

- why an approach exists
- when it makes sense
- where it breaks down

Topics are written as chapters, not tutorials.
They provide context, not step-by-step instructions.

---

### 2. Architecture Decision Records (ADRs)

ADRs capture **specific decisions** made at a point in time.

Each ADR documents:

- the problem being solved
- the decision taken
- alternatives that were considered
- consequences and trade-offs

ADRs are opinionated and contextual.
They may change as experience grows.

If you want to understand _why_ something was chosen, read the ADR.

---

### 3. Samples

Samples are small, runnable proofs.

They exist to:

- validate a decision
- demonstrate a boundary or constraint
- expose trade-offs in practice

Samples are intentionally minimal.
They are not demo applications.

Every sample should link back to the ADR it supports.

---

## Where to begin

If you are new to this repository, follow this order:

1. Read **ADR-0001** to understand why this repository exists.
2. Read **ADR-0002** to understand how it is structured.
3. Explore Topics that are relevant to your interests.
4. Use Samples only when you want to see a specific idea proven in code.

---

## What this repository is not

This repository does not aim to be:

- a framework or starter kit
- a “best practices” checklist
- a complete production reference system
- a tutorial series

The focus is on reasoning and decision-making, not prescriptions.

---

## Who this repository is for

This repository is intended for:

- senior .NET developers
- architects and tech leads
- engineers moving from implementation to design
- developers who value reasoning over dogma

---

## Final note

Disagreement is expected.

If a decision here makes you uncomfortable, question it.
Architecture improves when reasoning is challenged with experience.
