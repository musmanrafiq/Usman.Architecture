# Pattern Selector

Use this guide to pick the right design pattern for your problem.

---

## "I need to..."

### Control object creation

| Situation                                                          | Pattern              |
| ------------------------------------------------------------------ | -------------------- |
| Exactly one instance shared across the app (config, cache, logger) | **Singleton**        |
| Create an object without specifying the exact class                | **Factory Method**   |
| Create families of related objects that must work together         | **Abstract Factory** |
| Construct a complex object step-by-step with optional parts        | **Builder**          |
| Create copies of an existing object cheaply                        | **Prototype**        |

---

### Compose objects / adapt structure

| Situation                                                         | Pattern       |
| ----------------------------------------------------------------- | ------------- |
| Use a third-party/legacy class that has an incompatible interface | **Adapter**   |
| Add behaviour to an object at runtime without subclassing         | **Decorator** |
| Simplify a complex subsystem behind a single entry point          | **Facade**    |
| Control access to another object (auth, caching, lazy load)       | **Proxy**     |
| Treat individual objects and groups of objects uniformly          | **Composite** |

---

### Manage behaviour / communication

| Situation                                                     | Pattern                     |
| ------------------------------------------------------------- | --------------------------- |
| Notify multiple objects when something changes                | **Observer**                |
| Choose an algorithm at runtime without changing the caller    | **Strategy**                |
| Encapsulate an operation as an object with undo support       | **Command**                 |
| Pass a request along a chain until something handles it       | **Chain of Responsibility** |
| Decouple components by routing messages through a central hub | **Mediator**                |

---

## Real-world Scenario Map

| Scenario                                             | Primary Patterns                    | Location                  |
| ---------------------------------------------------- | ----------------------------------- | ------------------------- |
| Caching exchange rates / query results               | Singleton + Proxy                   | `Scenarios/Caching`       |
| Structured, enriched logging                         | Decorator + Chain of Responsibility | `Scenarios/Logging`       |
| Multi-gateway payment processing                     | Strategy + Factory                  | `Scenarios/Payments`      |
| User notification preferences                        | Observer + Strategy                 | `Scenarios/Notifications` |
| HTTP request pipeline (auth, rate-limiting, routing) | Chain of Responsibility + Decorator | `Scenarios/Middleware`    |

---

## Quick Decision Tree

```
Do you need ONE shared instance?
  └─ YES → Singleton
  └─ NO
      └─ Creating objects?
            └─ YES → Factory / AbstractFactory / Builder / Prototype
            └─ NO
                └─ Changing structure?
                      └─ YES → Adapter / Decorator / Facade / Proxy / Composite
                      └─ NO  → Observer / Strategy / Command / CoR / Mediator
```

---

## Anti-Pattern Warnings

| Don't                                  | Use Instead                             |
| -------------------------------------- | --------------------------------------- |
| Singleton with mutable shared state    | Scoped DI lifetime or immutable config  |
| Giant `if/switch` to pick behaviour    | Strategy or Factory                     |
| Deeply nested callbacks                | Chain of Responsibility or Mediator     |
| God-class doing everything             | Facade (split it) + separate subsystems |
| Adding features by subclassing forever | Decorator                               |
