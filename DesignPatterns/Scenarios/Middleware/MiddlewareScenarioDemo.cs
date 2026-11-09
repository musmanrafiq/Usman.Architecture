namespace Scenarios.Middleware;

public static class MiddlewareScenarioDemo
{
    public static async Task RunAsync()
    {
        var pipeline = new Pipeline();

        Console.WriteLine("=== Request 1: unauthenticated ===");
        var ctx1 = new HttpContext { Method = "GET", Path = "/api/orders" };
        await pipeline.ExecuteAsync(ctx1);

        Console.WriteLine("\n=== Request 2: authenticated ===");
        var ctx2 = new HttpContext { Method = "GET", Path = "/api/orders" };
        ctx2.Headers["Authorization"] = "Bearer valid-token-123";
        await pipeline.ExecuteAsync(ctx2);
    }
}
