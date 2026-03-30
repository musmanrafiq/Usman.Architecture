namespace Scenarios.Logging;

public interface IUserService
{
    Task<string> CreateUserAsync(string email);
}
