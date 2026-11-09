# Design Patterns

Practical, runnable examples of the Gang of Four design patterns — organised by category and mapped to real-world scenarios.

---

## Structure

```
DesignPatterns/
├── Creational/               Classic object-creation patterns
│   ├── Singleton/            AppConfiguration — thread-safe Lazy<T>
│   ├── Factory/              NotificationSenderFactory — Email, SMS, Push
│   ├── AbstractFactory/      UiFactory — Windows vs. Mac component families
│   ├── Builder/              HttpRequestBuilder — fluent, validated construction
│   └── Prototype/            DocumentTemplate — deep-clone with independent state
│
├── Structural/               Object composition & interface adaptation
│   ├── Adapter/              PayPalAdapter — wrap legacy SDK behind IPaymentGateway
│   ├── Decorator/            LoggingOrderService + MetricsOrderService — composable cross-cutting
│   ├── Facade/               CheckoutFacade — single entry point over 4 subsystems
│   ├── Proxy/                CachingProductRepository — transparent DB caching
│   └── Composite/            FileSystemItem — uniform treatment of files and directories
│
├── Behavioral/               Object communication & responsibility
│   ├── Observer/             OrderEventDispatcher — email, loyalty, fraud on order placed
│   ├── Strategy/             ShippingCalculator — Standard, Express, International
│   ├── Command/              CartCommandInvoker — add with full undo support
│   ├── ChainOfResponsibility/ SupportHandler — L1 → L2 → L3 escalation
│   └── Mediator/             Mediator — lightweight CQRS-style request/handler dispatch
│
├── Scenarios/                ⭐ Real-world combinations of multiple patterns
│   ├── Caching/              Singleton + Proxy → cached exchange rate service
│   ├── Logging/              Decorator + CoR  → enrichment pipeline + service decorator
│   ├── Payments/             Strategy + Factory → Stripe / PayPal / BankTransfer at runtime
│   ├── Notifications/        Observer + Strategy → per-user channel routing (email/SMS/push)
│   └── Middleware/           CoR + Decorator → ASP.NET Core-style auth/ratelimit/routing pipeline
│
├── PatternSelector.md        Quick-reference: which pattern solves my problem?
└── README.md                 This file
```

---

## How to Run Any Example

Each project contains a `*Demo` static class with a `Run()` or `RunAsync()` method.

```csharp
// Example — run from a console app or test
await Scenarios.Payments.PaymentsScenarioDemo.RunAsync();
Behavioral.ChainOfResponsibility.ChainDemo.Run();
```

All projects target **net9.0** and have **no external dependencies** — just open and run.

---

## Building

```bash
# Build a single category
dotnet build DesignPatterns/Creational/Creational.sln

# Build all
for sln in Creational Structural Behavioral Scenarios; do
  dotnet build DesignPatterns/$sln/$sln.sln
done
```

---

## Learning Path

| Step | What to study                                                | Why                                             |
| ---- | ------------------------------------------------------------ | ----------------------------------------------- |
| 1    | **Creational** — start with Singleton → Builder              | Fundamental, used everywhere                    |
| 2    | **Structural** — Decorator → Proxy                           | Foundation for cross-cutting concerns           |
| 3    | **Behavioral** — Strategy → Observer → ChainOfResponsibility | Core of business logic flexibility              |
| 4    | **Scenarios** (all)                                          | See how patterns combine in production contexts |

---

## Key Principles Demonstrated

- **Open/Closed**: add a new payment provider by creating a new `IPaymentStrategy` — no existing code changes.
- **Single Responsibility**: each middleware stage does exactly one thing.
- **Dependency Inversion**: every scenario is wired through interfaces, not concrete types.
- **Composition over Inheritance**: Decorator and Chain of Responsibility show this explicitly.

---

See [PatternSelector.md](./PatternSelector.md) to find the right pattern for your problem.  
See [docs/PatternsInUse.md](../docs/PatternsInUse.md) for where these appear in the actual architecture.
