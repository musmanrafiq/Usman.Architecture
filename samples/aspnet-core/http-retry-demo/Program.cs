using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// ─── Register a typed HttpClient with a retry policy ─────────────────────────
//
// ADR-0021: 3 retries, exponential back-off + jitter, handles 5xx / 408 / 429.
// Only wire this up on idempotent endpoints (GET, HEAD, etc.).

builder.Services
    .AddHttpClient<IWeatherClient, WeatherClient>(c =>
    {
        c.BaseAddress = new Uri("https://api.open-meteo.com/");
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(RetryPolicies.HttpRetryWithLogging(
        builder.Services.BuildServiceProvider()
               .GetRequiredService<ILogger<WeatherClient>>()));

var app = builder.Build();

// ─── Demo endpoint ────────────────────────────────────────────────────────────
app.MapGet("/weather", async (IWeatherClient client) =>
    await client.GetForecastAsync());

app.Run();
