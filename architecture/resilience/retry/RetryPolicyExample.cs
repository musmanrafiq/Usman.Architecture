using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Architecture.Resilience.Retry;

/// <summary>
/// ADR-0021 — Demonstrates retry policy configurations using Polly.
/// Only apply to idempotent operations. Never use for payments or order creation.
/// </summary>
public static class RetryPolicyExample
{
    // ─── 1. Basic retry (3 attempts, no delay) ───────────────────────────────

    public static AsyncRetryPolicy BasicRetry() =>
        Policy
            .Handle<HttpRequestException>()
            .RetryAsync(retryCount: 3);

    // ─── 2. Exponential back-off (2s → 4s → 8s) ─────────────────────────────

    public static AsyncRetryPolicy ExponentialBackoff() =>
        Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    // ─── 3. Exponential back-off + jitter (recommended for production) ───────

    public static AsyncRetryPolicy ExponentialBackoffWithJitter()
    {
        var jitter = new Random();
        return Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt))
                    + TimeSpan.FromMilliseconds(jitter.Next(0, 300)));
    }

    // ─── 4. HTTP-specific policy (5xx + 408 + 429) ───────────────────────────

    public static IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()                               // 5xx, 408
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests) // 429
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    // ─── 5. HTTP policy with logging ─────────────────────────────────────────

    public static IAsyncPolicy<HttpResponseMessage> HttpRetryPolicyWithLogging(
        ILogger logger) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt))
                    + TimeSpan.FromMilliseconds(new Random().Next(0, 300)),
                onRetry: (outcome, timespan, attempt, _) =>
                {
                    logger.LogWarning(
                        "Retry {Attempt} after {Delay}s — Reason: {Reason}",
                        attempt,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                });
}
