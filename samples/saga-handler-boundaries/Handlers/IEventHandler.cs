// ADR-0030 -- EventHandler interface
using System.Threading;
using System.Threading.Tasks;

namespace SagaHandlerBoundaries.Handlers;

public interface IEventHandler<in TEvent>
{
    Task Handle(TEvent evt, CancellationToken ct = default);
}
