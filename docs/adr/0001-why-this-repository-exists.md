# ADR-0001: Why this repository exists

- Status: In-Progress
- Date: 2026-02-06
- By: Usman Rafiq

## Context

Most architecture content I’ve seen in the .NET world falls into familiar patterns:

- tool-first tutorials that explain _how_ but rarely _why_
- rigid rules and checklists that ignore context
- “best practices” stated with confidence but without real-world proof

In actual systems, architecture is rarely clean or theoretical.

It is shaped by:

- deadlines and team skills
- legacy systems that can’t be rewritten
- operational realities like deployment, monitoring, and support
- changing business requirements

Architectural decisions are usually made under pressure and then forgotten.
Months later, when problems appear, the original reasoning is gone.

This repository exists because I wanted a better way to:

- think through architectural decisions
- document why a choice was made
- capture alternatives and trade-offs
- validate ideas with small, working code instead of opinions

## Decision

I will maintain this repository as a **living architecture notebook**.

It is not meant to be polished or final.
It is meant to reflect how architectural understanding evolves over time.

This repository focuses on **thinking and decisions**, supported by code when needed.

It will contain:

- **Architecture topics** that explain _why_ and _when_, not just _how_
- **Architecture Decision Records (ADRs)** that capture real decisions and their consequences
- **Small, runnable samples** that demonstrate one idea at a time

This repository is deliberately **not**:

- a framework or starter template
- a “clean architecture” guide
- a large demo app pretending to be production-ready

Larger, production-style systems may live in separate repositories and be referenced from here when they help explain a decision.

## Alternatives considered

### Option 1: Build one big reference application in this repository

**Why it was tempting**

- everything in one place
- easy to show features quickly

**Why I rejected it**

- architectural reasoning gets buried under application details
- refactoring the app invalidates older conclusions
- it becomes hard to explain _why_ things exist the way they do

---

### Option 2: Organize content around tools (EF Core, MediatR, messaging, etc.)

**Why it was tempting**

- easy to search
- aligns with how most tutorials are written

**Why I rejected it**

- encourages tool-first thinking
- architecture becomes a list of libraries
- the context behind decisions gets lost

---

### Option 3: Keep architecture notes private

**Why it was tempting**

- less effort
- no need to explain or justify decisions

**Why I rejected it**

- writing stays shallow
- decisions aren’t revisited
- no long-term record of how thinking evolved

## Consequences

### What this enables

- Decisions are explicit and easier to revisit.
- Trade-offs are visible instead of implied.
- Code exists to support ideas, not dominate them.
- The repository stays readable over time.

### What this costs

- Progress may feel slower than building a big demo app.
- Samples must stay small and focused.
- Some readers will expect tutorials or templates and won’t find them here.

## Guiding principles

- Architecture is about constraints, not perfection.
- Boundaries matter more than layers.
- Code structure should reflect business intent.
- Tools support architecture — not the other way around.
- Decisions are contextual and allowed to change.

## Notes

This ADR is not permanent truth.

If experience proves this approach wrong, the decision can be revised.
That revision should be documented — not hidden.
