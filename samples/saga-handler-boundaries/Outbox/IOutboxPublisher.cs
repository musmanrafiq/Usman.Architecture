// ADR-0030 -- IOutboxPublisher interface
using System.Threading;
using System.Threading.Tasks;

namespace SagaHandlerBoundaries.Outbox;

public interface IOutboxPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class;
}
