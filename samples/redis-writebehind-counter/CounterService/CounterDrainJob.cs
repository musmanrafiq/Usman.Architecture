using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RedisWriteBehindCounter.CounterService;

/// <summary>
/// Background service that periodically drains aggregated counters from Redis
/// into the SQL database in batches.
///
/// This is a singleton (not registered as a hosted service in the generic sense).
/// It uses <see cref="PeriodicTimer"/> to control the drain interval and a
/// <see cref="SemaphoreSlim"/> to ensure only one process drains at a time.
/// </summary>
public class CounterDrainJob : BackgroundService
{
    private readonly ISemaphoreSlim _drainLock = new(1, 1);
    private readonly PeriodicTimer _timer;
    private readonly ICounterStore _redis;
    private readonly ISqlCounterStore _sql;
    private readonly ILogger<CounterDrainJob> _logger;

    /// <summary>
    /// Interval between drain attempts.
    /// </summary>
    public TimeSpan DrainInterval { get; init; } = TimeSpan.FromSeconds(30);

    public CounterDrainJob(
        ICounterStore redis,
        ISqlCounterStore sql,
        ILogger<CounterDrainJob> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timer = new PeriodicTimer(DrainInterval);
    }

    public override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CounterDrainJob starting (interval: {Interval}s)", DrainInterval.TotalSeconds);

        try
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                await DrainOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("CounterDrainJob stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CounterDrainJob unexpected failure");
        }
    }

    private async Task DrainOnceAsync(CancellationToken ct)
    {
        // 1. Acquire the distributed lock to prevent multiple instances from draining simultaneously
        if (!await _drainLock.WaitAsync(0, ct))
        {
            _logger.LogDebug("Another drain instance is active — skipping this tick");
            return;
        }

        try
        {
            _logger.LogDebug("Starting drain batch");

            // 2. Scan Redis for pending counters
            var batches = await _redis.ScanPendingBatchAsync(ct);

            if (batches.Count == 0)
            {
                _logger.LogDebug("No pending counters to drain");
                return;
            }

            // 3. Bulk insert into SQL
            var inserted = await _sql.BulkInsertAsync(batches, ct);
            _logger.LogInformation("Drained {Count} counter records", inserted);

            // 4. Remove drained keys from Redis
            await _redis.RemoveDrainedKeysAsync(batches.Select(b => BuildKey(b)), ct);

            _logger.LogDebug("Drain batch completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Counter drain failed");
            // Exception re-thrown by try/catch → will be retried on next tick
        }
        finally
        {
            _drainLock.Release();
        }
    }

    private static string BuildKey(CounterBatch batch)
        => $"counter:{batch.CounterType}:{(batch.Granularity)}:{batch.DimensionKey}:{""}}";
}

