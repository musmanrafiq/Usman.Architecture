namespace Scenarios.Middleware;

public class AuthenticationMiddleware : MiddlewareBase
{
    public override async Task InvokeAsync(HttpContext ctx)
    {
        if (!ctx.Headers.ContainsKey("Authorization"))
        {
            Console.WriteLine("[Auth] Missing token - 401 Unauthorized");
            ctx.StatusCode = 401;
            ctx.Aborted = true;
            return;
        }

        Console.WriteLine("[Auth] Token valid - proceeding");
        if (_next is not null) await _next(ctx);
    }
}
