namespace Scenarios.Caching;

/// <summary>
/// Thread-safe Singleton cache store backed by a simple in-memory dictionary.
/// Used by CachedExchangeRateService as the shared cache instance.
/// </summary>
public sealed class InMemoryCache
{
    private static readonly Lazy<InMemoryCache> _instance = new(() => new InMemoryCache());
    public static InMemoryCache Instance => _instance.Value;

    private readonly Dictionary<string, (object Value, DateTime ExpiresAt)> _store = [];

    private InMemoryCache() { }

    public void Set(string key, object value, TimeSpan ttl)
        => _store[key] = (value, DateTime.UtcNow.Add(ttl));

    public bool TryGet<T>(string key, out T? value)
    {
        if (_store.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
        {
            value = (T)entry.Value;
            return true;
        }
        _store.Remove(key);
        value = default;
        return false;
    }

    public void Invalidate(string key) => _store.Remove(key);
}
