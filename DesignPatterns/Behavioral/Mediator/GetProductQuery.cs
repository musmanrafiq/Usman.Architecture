namespace Behavioral.Mediator;

public record GetProductQuery(int ProductId) : IRequest<ProductDto>;
