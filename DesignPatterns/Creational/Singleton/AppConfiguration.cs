namespace Creational.Singleton;

/// <summary>
/// Thread-safe Singleton using Lazy&lt;T&gt;.
/// Real-world use: configuration manager loaded once per app lifetime.
/// </summary>
public sealed class AppConfiguration
{
    private static readonly Lazy<AppConfiguration> _instance =
        new(() => new AppConfiguration(), isThreadSafe: true);

    public static AppConfiguration Instance => _instance.Value;

    public string DatabaseConnectionString { get; private set; }
    public string ApiBaseUrl { get; private set; }
    public int MaxRetryAttempts { get; private set; }

    private AppConfiguration()
    {
        // In production, load from IConfiguration / environment variables
        DatabaseConnectionString = "Server=localhost;Database=AppDb;Trusted_Connection=True";
        ApiBaseUrl = "https://api.example.com";
        MaxRetryAttempts = 3;
    }
}
