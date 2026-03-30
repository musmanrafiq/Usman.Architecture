using System.Net;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace HttpRetryDemo;

/// <summary>
/// Centralised retry policies — import once, reference everywhere.
/// ADR-0021: exponential back-off (2ⁿ seconds) + jitter, max 3 retries.
/// </summary>
public static class RetryPolicies
{
    /// <summary>
    /// Production-ready HTTP retry policy.
    /// Handles: 5xx, 408 (Request Timeout), 429 (Too Many Requests).
    /// Adds jitter to reduce thundering-herd / retry storms.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> HttpRetryWithLogging(ILogger logger)
    {
        var jitter = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()                                    // 5xx + 408
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests) // 429
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt))             // 2s, 4s, 8s
                    + TimeSpan.FromMilliseconds(jitter.Next(0, 300)),      // ± jitter
                onRetry: (outcome, delay, attempt, _) =>
                {
                    var reason = outcome.Exception?.Message
                                 ?? outcome.Result.StatusCode.ToString();

                    logger.LogWarning(
                        "[Retry {Attempt}/3] Waiting {Delay:N1}s before next attempt. Reason: {Reason}",
                        attempt, delay.TotalSeconds, reason);
                });
    }
}
