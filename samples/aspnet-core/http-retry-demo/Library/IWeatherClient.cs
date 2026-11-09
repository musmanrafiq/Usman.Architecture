namespace HttpRetryDemo.Library;

public interface IWeatherClient
{
    Task<string> GetForecastAsync();
}