namespace Scenarios.Caching;

public static class CachingScenarioDemo
{
    public static async Task RunAsync()
    {
        IExchangeRateService svc = new CachedExchangeRateService(
            new ExchangeRateService(), TimeSpan.FromMinutes(5));

        var r1 = await svc.GetRateAsync("GBP", "USD"); // API call
        var r2 = await svc.GetRateAsync("GBP", "USD"); // cache hit
        var r3 = await svc.GetRateAsync("GBP", "USD"); // cache hit

        Console.WriteLine($"Rate: {r1.Rate} | Fetched: {r1.FetchedAt:T}");
        Console.WriteLine($"All 3 calls returned same rate: {r1.Rate == r2.Rate && r2.Rate == r3.Rate}");
    }
}
