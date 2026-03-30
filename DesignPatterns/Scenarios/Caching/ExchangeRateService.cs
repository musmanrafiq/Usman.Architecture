namespace Scenarios.Caching;

/// <summary>Real implementation — simulates a slow external HTTP call.</summary>
public class ExchangeRateService : IExchangeRateService
{
    public async Task<ExchangeRate> GetRateAsync(string from, string to)
    {
        await Task.Delay(300); // simulate HTTP latency
        Console.WriteLine($"[API] Fetching {from}/{to} exchange rate from remote...");
        return new ExchangeRate(from, to, 1.27m, DateTime.UtcNow);
    }
}
