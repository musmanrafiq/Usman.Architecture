namespace Creational.Builder;

public static class BuilderDemo
{
    public static void Run()
    {
        var request = new HttpRequestBuilder()
            .WithMethod("POST")
            .WithUrl("https://api.example.com/orders")
            .WithBearerToken("my-secret-token")
            .WithHeader("Content-Type", "application/json")
            .WithBody("{\"productId\": 42, \"quantity\": 1}")
            .WithTimeout(10)
            .Build();

        Console.WriteLine(request);
    }
}
