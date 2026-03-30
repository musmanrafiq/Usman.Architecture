namespace HttpRetryDemo;

public interface IWeatherClient
{
    Task<string> GetForecastAsync();
}

/// <summary>
/// Typed HttpClient — the retry policy is applied at registration time in Program.cs.
/// This class stays clean and has no retry logic of its own.
/// </summary>
public sealed class WeatherClient(HttpClient http, ILogger<WeatherClient> logger)
    : IWeatherClient
{
    // Free, no-key Open-Meteo API — safe for local demos.
    private const string ForecastUrl =
        "v1/forecast?latitude=51.5074&longitude=-0.1278&current_weather=true";

    public async Task<string> GetForecastAsync()
    {
        logger.LogInformation("Fetching weather forecast…");

        // The HttpClient here already has the Polly retry handler attached.
        var response = await http.GetAsync(ForecastUrl);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
