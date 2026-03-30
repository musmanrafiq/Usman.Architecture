namespace Structural.Proxy;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
}
