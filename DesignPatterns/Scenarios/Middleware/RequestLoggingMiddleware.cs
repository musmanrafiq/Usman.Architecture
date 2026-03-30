namespace Scenarios.Middleware;

public class RequestLoggingMiddleware : MiddlewareBase
{
    public override async Task InvokeAsync(HttpContext ctx)
    {
        Console.WriteLine($"[Log] --> {ctx.Method} {ctx.Path}");
        if (_next is not null) await _next(ctx);
        Console.WriteLine($"[Log] <-- {ctx.StatusCode}");
    }
}
