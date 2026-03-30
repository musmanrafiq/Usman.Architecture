namespace Structural.Proxy;

/// <summary>
/// Wraps IProductRepository with an in-memory cache.
/// The caller never knows whether it hit the DB or the cache.
/// </summary>
public class CachingProductRepository(IProductRepository inner) : IProductRepository
{
    private readonly Dictionary<int, Product> _cache = [];

    public async Task<Product?> GetByIdAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            Console.WriteLine($"[CACHE] Hit for product {id}");
            return cached;
        }

        var product = await inner.GetByIdAsync(id);
        if (product is not null)
            _cache[id] = product;

        return product;
    }
}
