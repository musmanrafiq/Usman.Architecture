namespace Structural.Proxy;

public static class ProxyDemo
{
    public static async Task RunAsync()
    {
        IProductRepository repo = new CachingProductRepository(new ProductRepository());

        var p1 = await repo.GetByIdAsync(1); // DB hit
        var p2 = await repo.GetByIdAsync(1); // cache hit
        var p3 = await repo.GetByIdAsync(2); // DB hit

        Console.WriteLine($"Products: {p1?.Name}, {p2?.Name}, {p3?.Name}");
    }
}
