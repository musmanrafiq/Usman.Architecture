namespace Scenarios.Middleware;

public class RateLimitMiddleware : MiddlewareBase
{
    private static int _requestCount;
    private const int MaxRequests = 5;

    public override async Task InvokeAsync(HttpContext ctx)
    {
        if (++_requestCount > MaxRequests)
        {
            Console.WriteLine($"[RateLimit] Too many requests ({_requestCount}) - 429");
            ctx.StatusCode = 429;
            ctx.Aborted = true;
            return;
        }

        Console.WriteLine($"[RateLimit] Request {_requestCount}/{MaxRequests} allowed");
        if (_next is not null) await _next(ctx);
    }
}
