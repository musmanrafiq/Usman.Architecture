// ADR-0030 -- In-memory outbox publisher for demo
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SagaHandlerBoundaries.Outbox;

public class InMemoryOutboxPublisher : IOutboxPublisher
