namespace Structural.Proxy;

public class ProductRepository : IProductRepository
{
    private static readonly Dictionary<int, Product> _db = new()
    {
        [1] = new Product(1, "Widget Pro", 29.99m),
        [2] = new Product(2, "Gadget Ultra", 49.99m)
    };

    public async Task<Product?> GetByIdAsync(int id)
    {
        await Task.Delay(100); // simulate DB latency
        Console.WriteLine($"[DB] Fetching product {id} from database...");
        return _db.TryGetValue(id, out var p) ? p : null;
    }
}
