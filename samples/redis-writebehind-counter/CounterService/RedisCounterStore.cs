using StackExchange.Redis;

namespace RedisWriteBehindCounter.CounterService;

/// <summary>
/// Redis-backed write-behind counter store.
///
/// Counters are incremented in Redis using HINCRBY (hashed per tenant + counter type)
/// and flushed to SQL in batches on a timer.
/// </summary>
public class RedisCounterStore : ICounterStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCounterStore> _logger;

    public RedisCounterStore(IConnectionMultiplexer redis, ILogger<RedisCounterStore> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _db = redis.GetDatabase();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<long> IncrementAsync(
        string tenantId,
        string counterType,
        string dimensionKey,
        string granularity,
        long count = 1)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(counterType);
        ArgumentNullException.ThrowIfNull(granularity);

        // Redis key: counter:{counterType}:{granularity}:{tenantId}:{dimensionKey}
        var key = BuildKey(counterType, granularity, tenantId, dimensionKey);

        // HINCRBY atomically increments and returns the new value
        var newValue = await _db.HashIncrementAsync(key, "count", count);

        _logger.LogDebug(
            "Incremented counter {CounterType}/{Granularity}/{TenantId}/{DimensionKey} to {NewValue}",
            counterType, granularity, tenantId, dimensionKey, newValue);

        return newValue;
    }

    public async Task<IReadOnlyList<CounterBatch>> ScanPendingBatchAsync(CancellationToken ct)
    {
        var batches = new List<CounterBatch>();

        // Use SCAN (not KEYS) to safely iterate without blocking the Redis server
        var scanner = _db.Multiplexer.CreateScanChannel();
        await foreach (var pattern in scanner.ScanAsync("counter:*", ct))
        {
            var key = pattern!;
            var db = _redis.GetDatabase();

            var values = await db.HashGetAsync(key, "count");
            if (values.Length == 0) continue;

            var info = await db.KeyInfoAsync(key);
            if (info is null || !info.TryGetValue("type", out var typeStr) || typeStr != "hash")
                continue;

            var type = ExtractSegment(key, 1);
            var granularity = ExtractSegment(key, 2);
            var tenantId = ExtractSegment(key, 3);
            var dimensionKey = ExtractSegment(key, 4);

            if (long.TryParse(values[0], out var value))
            {
                batches.Add(new CounterBatch(type, granularity, tenantId, value, DateTimeOffset.UtcNow));
            }

            if (batches.Count >= 500) break; // Cap per-batch size to avoid oversized transactions
        }

        return batches;
    }

    public async Task RemoveDrainedKeysAsync(IEnumerable<string> drainedKeys, CancellationToken ct)
    {
        var db = _redis.GetDatabase();

        var keys = drainedKeys as string[] ?? drainedKeys.ToArray();
        if (keys.Length == 0) return;

        // Work in chunks to avoid sending huge command batches
        const int chunkSize = 500;
        for (int i = 0; i < keys.Length; i += chunkSize)
        {
            var chunk = keys.Skip(i).Take(chunkSize).ToArray();
            await db.KeyDeleteAsync(chunk, ct);
        }

        _logger.LogDebug("Removed {Count} drained Redis keys", keys.Count());
    }

    private static string BuildKey(string counterType, string granularity, string tenantId, string dimensionKey)
        => $"counter:{counterType}:{granularity}:{tenantId}:{dimensionKey}";

    private static string ExtractSegment(string key, int segmentIndex)
    {
        var parts = key.Split(':');
        return parts.Length > segmentIndex ? parts[segmentIndex] : string.Empty;
    }
}
