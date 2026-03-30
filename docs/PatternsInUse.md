# Patterns In Use

This document maps every design pattern implemented in this repository to where it appears — or should appear — in a real production modular monolith.

---

## Pattern Usage Map

### Singleton

**In this repo:** `Creational/Singleton` — `AppConfiguration`  
**In production:**

- `IConfiguration` binding class loaded once at startup
- `InMemoryCache` (see `Scenarios/Caching`)
- Connection pool / `HttpClientFactory` registration

```csharp
// Registered once in DI
builder.Services.AddSingleton<IAppConfiguration, AppConfiguration>();
```

---

### Factory Method

**In this repo:** `Creational/Factory` — `NotificationSenderFactory`  
**In production:**

- `INotificationSender` resolved per channel at runtime
- `IPaymentStrategy` selected based on user's saved payment method
- Feature flag driven object creation

```csharp
var sender = NotificationSenderFactory.Create(user.PreferredChannel);
```

---

### Abstract Factory

**In this repo:** `Creational/AbstractFactory` — `IUiFactory`  
**In production:**

- Multi-tenant themes (each tenant gets a `IUiFactory` that matches their brand)
- Cloud provider abstraction (`IAzureResourceFactory` vs `IAwsResourceFactory`)

---

### Builder

**In this repo:** `Creational/Builder` — `HttpRequestBuilder`  
**In production:**

- `IQueryBuilder` for dynamic data-layer queries
- Email/notification message construction with optional CC, attachments, templates
- Test data builders (`OrderBuilder`, `CustomerBuilder` in test projects)

```csharp
var request = new HttpRequestBuilder()
    .WithMethod("POST")
    .WithUrl(config.ApiBaseUrl + "/orders")
    .WithBearerToken(accessToken)
    .WithTimeout(10)
    .Build();
```

---

### Prototype

**In this repo:** `Creational/Prototype` — `DocumentTemplate`  
**In production:**

- Document / report template cloning (invoice templates per region)
- Snapshot of aggregate root for optimistic concurrency checks
- Copying `OrderLine` entries when creating recurring orders

---

### Adapter

**In this repo:** `Structural/Adapter` — `PayPalAdapter`  
**In production:**

- Wrapping a legacy `.asmx` SOAP payment service behind `IPaymentGateway`
- Adapting a third-party SMS SDK behind `INotificationSender`
- Anti-Corruption Layer between modules (e.g. `Accounts` adapts `Transactions.Contracts`)

---

### Decorator

**In this repo:** `Structural/Decorator` — `LoggingOrderService`, `MetricsOrderService`  
**In production:**

- Logging, metrics, and retry decorators on any `IRepository` or service
- Idempotency decorator on command handlers
- Authorisation decorator on query handlers

```csharp
// Registration order matters — outermost decorator runs first
services.AddScoped<IOrderService, OrderService>();
services.Decorate<IOrderService, LoggingOrderService>();
services.Decorate<IOrderService, MetricsOrderService>();
```

> Use **Scrutor** (`services.Decorate<T, TDecorator>()`) for clean decorator registration in ASP.NET Core.

---

### Facade

**In this repo:** `Structural/Facade` — `CheckoutFacade`  
**In production:**

- `CheckoutFacade` coordinating Inventory, Payment, Shipping, Notification
- `AccountRegistrationFacade` calling Identity, Profile, Welcome-email subsystems
- Application Service layer in a modular monolith (the service IS the facade)

---

### Proxy

**In this repo:** `Structural/Proxy` — `CachingProductRepository`  
**In production:**

- `IDistributedCache`-backed proxy for any `IRepository`
- Authorisation proxy: checks permissions before forwarding to real service
- Lazy-loading proxy in ORM (e.g. EF Core navigation property loading)

---

### Composite

**In this repo:** `Structural/Composite` — `FileSystemItem`  
**In production:**

- Menu / navigation tree rendering
- Discount rule trees in an e-commerce engine (each rule can contain sub-rules)
- Org-chart / reporting hierarchy traversal

---

### Observer

**In this repo:** `Behavioral/Observer` — `OrderEventDispatcher`  
**In production:**

- Domain events dispatched after `SaveChangesAsync` (MediatR / custom dispatcher)
- `INotificationHandler<OrderPlacedEvent>` → Email, Loyalty, Fraud
- Outbox pattern: observers write to outbox, background worker delivers

```csharp
// MediatR notification = Observer pattern
public class SendConfirmationEmail : INotificationHandler<OrderPlacedEvent>
{
    public Task Handle(OrderPlacedEvent notification, CancellationToken ct) { ... }
}
```

---

### Strategy

**In this repo:** `Behavioral/Strategy` — `ShippingCalculator`  
**In production:**

- Shipping cost calculation (Standard / Express / International)
- Discount engine: `IDiscountStrategy` selected per customer tier
- Export format: `IExportStrategy` (CSV, Excel, PDF) selected from user preference
- Tax calculation: per-country `ITaxStrategy`

---

### Command

**In this repo:** `Behavioral/Command` — `CartCommandInvoker`  
**In production:**

- Every CQRS command is a Command pattern object
- Undo/redo in document editors
- Job queue: serialise command objects to a message broker (RabbitMQ / Azure Service Bus)

---

### Chain of Responsibility

**In this repo:** `Behavioral/ChainOfResponsibility` — `SupportHandler`  
**In production:**

- ASP.NET Core middleware pipeline (see `Scenarios/Middleware`)
- Approval workflows: Manager → Director → VP
- Validation pipeline: each validator in the chain can short-circuit
- Log enrichment pipeline (see `Scenarios/Logging`)

---

### Mediator

**In this repo:** `Behavioral/Mediator` — `Mediator` (hand-rolled)  
**In production:**

- **MediatR** library implements this pattern for CQRS (`IRequest<T>` / `IRequestHandler<T, R>`)
- Every feature vertical in a modular monolith uses a mediator to avoid direct service coupling

```csharp
var product = await _mediator.Send(new GetProductQuery(productId));
```

---

## Pattern Frequency in a Typical Module

| Layer                   | Most Used Patterns                                                  |
| ----------------------- | ------------------------------------------------------------------- |
| Application (Use Cases) | **Mediator**, Command, Observer (domain events)                     |
| Domain                  | Strategy, Composite, Prototype                                      |
| Infrastructure          | Adapter, Proxy (caching/retry), Decorator                           |
| API / Presentation      | Facade (Application Services), Chain of Responsibility (middleware) |
| DI Composition Root     | Factory, Singleton, Decorator                                       |

---

## Cross-cutting Concerns

| Concern       | Pattern             | Implementation            |
| ------------- | ------------------- | ------------------------- |
| Caching       | Singleton + Proxy   | `Scenarios/Caching`       |
| Logging       | Decorator + CoR     | `Scenarios/Logging`       |
| Payments      | Strategy + Factory  | `Scenarios/Payments`      |
| Notifications | Observer + Strategy | `Scenarios/Notifications` |
| Middleware    | CoR + Decorator     | `Scenarios/Middleware`    |

---

See [DesignPatterns/PatternSelector.md](../DesignPatterns/PatternSelector.md) to choose the right pattern.
