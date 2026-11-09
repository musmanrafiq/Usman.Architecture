namespace Scenarios.Middleware;

/// <summary>
/// Assembles the middleware chain in the correct order.
/// Add or remove stages here without touching any individual middleware.
/// </summary>
public class Pipeline
{
    private readonly MiddlewareBase _first;

    public Pipeline()
    {
        var logging = new RequestLoggingMiddleware();
        var auth = new AuthenticationMiddleware();
        var rateLimit = new RateLimitMiddleware();
        var router = new RouteHandlerMiddleware();

        logging.Use(auth).Use(rateLimit).Use(router);
        _first = logging;
    }

    public Task ExecuteAsync(HttpContext context) => _first.InvokeAsync(context);
}
