namespace Scenarios.Caching;

public record ExchangeRate(string From, string To, decimal Rate, DateTime FetchedAt);

public interface IExchangeRateService
{
    Task<ExchangeRate> GetRateAsync(string from, string to);
}
