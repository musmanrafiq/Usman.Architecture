namespace Scenarios.Logging;

public static class LoggingScenarioDemo
{
    public static async Task RunAsync()
    {
        var logger = new StructuredLogger();
        IUserService svc = new LoggingUserService(new UserService(), logger);

        var userId = await svc.CreateUserAsync("alice@example.com");
        Console.WriteLine($"Done. User ID: {userId}");
    }
}
