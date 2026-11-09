namespace Behavioral.Mediator;

/// <summary>
/// Lightweight mediator: decouples senders from handlers.
/// Register a handler per request type, then Send() routes to the right one.
/// </summary>
public class Mediator
{
    private readonly Dictionary<Type, object> _handlers = [];

    public void Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
        => _handlers[typeof(TRequest)] = handler;

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        if (!_handlers.TryGetValue(request.GetType(), out var handler))
            throw new InvalidOperationException($"No handler registered for {request.GetType().Name}");

        var method = handler.GetType().GetMethod("HandleAsync")!;
        return (Task<TResponse>)method.Invoke(handler, [request])!;
    }
}
