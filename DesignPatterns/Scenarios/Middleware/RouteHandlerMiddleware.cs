namespace Scenarios.Middleware;

public class RouteHandlerMiddleware : MiddlewareBase
{
    public override Task InvokeAsync(HttpContext ctx)
    {
        Console.WriteLine($"[Router] Handling {ctx.Path} -> 200 OK");
        ctx.StatusCode = 200;
        return Task.CompletedTask;
    }
}
