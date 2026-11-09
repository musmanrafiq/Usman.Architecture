namespace Scenarios.Logging;

/// <summary>
/// Decorator — adds structured logging around IUserService
/// without modifying UserService itself.
/// </summary>
public class LoggingUserService(IUserService inner, StructuredLogger logger) : IUserService
{
    public async Task<string> CreateUserAsync(string email)
    {
        logger.Log("INFO", $"Creating user: {email}");
        var result = await inner.CreateUserAsync(email);
        logger.Log("INFO", $"User created: {result}");
        return result;
    }
}
