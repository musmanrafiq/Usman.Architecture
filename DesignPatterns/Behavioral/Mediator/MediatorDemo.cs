namespace Behavioral.Mediator;

public static class MediatorDemo
{
    public static async Task RunAsync()
    {
        var mediator = new Mediator();
        mediator.Register<GetProductQuery, ProductDto>(new GetProductQueryHandler());

        var product = await mediator.SendAsync(new GetProductQuery(1));
        Console.WriteLine($"Result: {product.Name} @ {product.Price:C}");
    }
}
