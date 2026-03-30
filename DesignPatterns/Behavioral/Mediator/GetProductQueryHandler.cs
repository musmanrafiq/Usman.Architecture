namespace Behavioral.Mediator;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private static readonly Dictionary<int, ProductDto> _products = new()
    {
        [1] = new ProductDto(1, "Widget Pro", 29.99m),
        [2] = new ProductDto(2, "Gadget Ultra", 49.99m)
    };

    public Task<ProductDto> HandleAsync(GetProductQuery request)
    {
        if (!_products.TryGetValue(request.ProductId, out var product))
            throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        Console.WriteLine($"[Handler] Fetched product: {product.Name}");
        return Task.FromResult(product);
    }
}
