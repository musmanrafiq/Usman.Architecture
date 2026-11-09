namespace Scenarios.Caching;

/// <summary>
/// Proxy — wraps IExchangeRateService transparently.
/// Callers never know whether the response came from the API or the cache.
/// </summary>
public class CachedExchangeRateService(IExchangeRateService inner, TimeSpan ttl) : IExchangeRateService
{
    private readonly InMemoryCache _cache = InMemoryCache.Instance;

    public async Task<ExchangeRate> GetRateAsync(string from, string to)
    {
        var key = $"rate:{from}:{to}";

        if (_cache.TryGet<ExchangeRate>(key, out var cached))
        {
            Console.WriteLine($"[Cache HIT] {from}/{to}");
            return cached!;
        }

        var rate = await inner.GetRateAsync(from, to);
        _cache.Set(key, rate, ttl);
        Console.WriteLine($"[Cache SET] {from}/{to} (TTL: {ttl.TotalSeconds}s)");
        return rate;
    }
}
