namespace Creational.Singleton;

public static class SingletonDemo
{
    public static void Run()
    {
        var a = AppConfiguration.Instance;
        var b = AppConfiguration.Instance;

        Console.WriteLine($"Same instance: {ReferenceEquals(a, b)}"); // True
        Console.WriteLine($"API URL: {a.ApiBaseUrl}");
    }
}
