namespace Scenarios.Middleware;

/// <summary>
/// Abstract handler for the pipeline (Chain of Responsibility).
/// Use() chains the next stage; InvokeAsync() does the work then forwards.
/// </summary>
public abstract class MiddlewareBase
{
    protected RequestDelegate? _next;

    public delegate Task RequestDelegate(HttpContext context);

    public MiddlewareBase Use(MiddlewareBase next)
    {
        _next = next.InvokeAsync;
        return next;
    }

    public abstract Task InvokeAsync(HttpContext context);
}
