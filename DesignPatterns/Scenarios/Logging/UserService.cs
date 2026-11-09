namespace Scenarios.Logging;

public class UserService : IUserService
{
    public Task<string> CreateUserAsync(string email)
    {
        var id = $"USR-{Guid.NewGuid():N}"[..10];
        return Task.FromResult(id);
    }
}
