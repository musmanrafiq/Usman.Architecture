# Usman.Architecture

> My personal architecture notebook.  
> These are my learnings, pragmatic decisions, trade-offs, and working proofs from building real .NET systems.

---
Maintained by **Usman Rafiq** — https://usmancode.com

## What this repository is

This repository captures **how I think about software architecture** in the .NET ecosystem — shaped by real projects, real constraints, and real consequences.

It exists to:
- document architectural decisions
- explore alternatives and trade-offs
- validate ideas through small, runnable proofs
- revisit and revise conclusions over time

This is **not** a framework or a template.  
It is a living record of architectural thinking.

---

## What this repository is not

- ❌ A “clean architecture” checklist  
- ❌ A one-size-fits-all solution  
- ❌ A collection of buzzwords without context  
- ❌ A demo app pretending to be production  

Architecture here is contextual, opinionated, and revisable.

---

## How to navigate this repository

The repository is structured around **intent**, not tools.

### 1. Start Here
`docs/00-Start-Here/`

This section explains:
- the philosophy behind this repository
- how decisions are made
- the vocabulary used throughout the docs

If you are new here, start there.

---

### 2. Architecture Topics
`docs/topics/`

Topics are written as **chapters**, not tutorials.

Each topic focuses on *why* and *when*, not just *how*.

Examples include:
- Modular Monolith
- Microservices
- Messaging and integration
- Persistence with EF Core
- Application boundaries and module isolation

Technologies such as MediatR, EF Core, or NServiceBus appear **within topics**, not as the organizing principle.

---

### 3. Architecture Decision Records (ADRs)
`docs/adr/`

Every meaningful architectural decision is documented using ADRs.

Each ADR records:
- the context
- the decision
- alternatives considered
- consequences and trade-offs

ADRs are allowed to change when experience proves them wrong.

---

### 4. Runnable Proofs
`samples/`

Small, focused, runnable examples that demonstrate **one architectural idea at a time**.

Rules for samples:
- minimal scope
- clear intent
- directly referenced from docs or ADRs
- no “demo app” bloat

---

## Relationship to real applications

This repository focuses on **thinking, decisions, and proofs**.

Larger, production-style applications (for example, a modular monolith or a microservices system) may live in separate repositories and are referenced from here when relevant.

This keeps `Usman.Architecture` focused, readable, and long-lived.

---

## Guiding principles

- Architecture is about **constraints**, not purity  
- Boundaries matter more than layers  
- Code structure should reflect **business intent**  
- Tooling serves architecture — never the other way around  
- Decisions are contextual and revisable  

---

## Who this is for

- Senior .NET developers
- Architects and tech leads
- Engineers moving from “writing code” to “designing systems”
- Developers who want less dogma and more reasoning

---

## Status

This is a **living repository**.

Ideas evolve.  
Decisions change.  
Assumptions get challenged.

That is intentional.

---

## License

MIT — free to use, adapt, and reference.

---

## Final note

If you find yourself disagreeing with something here, that’s a good sign.

Architecture improves through **thoughtful disagreement backed by experience**.
