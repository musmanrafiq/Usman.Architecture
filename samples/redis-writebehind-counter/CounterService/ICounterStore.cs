using StackExchange.Redis;

namespace RedisWriteBehindCounter.CounterService;

/// <summary>
/// Defines the contract for a write-behind counter store backed by Redis.
/// </summary>
public interface ICounterStore
{
    /// <summary>
    /// Increment the counter for a given tenant, counter type, and granularity.
    /// </summary>
    /// <param name="tenantId">Tenant or user identifier.</param>
    /// <param name="counterType">e.g., "api_token_usage", "page_views", "feature_usage"</param>
    /// <param name="dimensionKey">Additional dimension key (e.g., feature name). May be empty.</param>
    /// <param name="granularity">e.g., "day", "hour", "minute".</param>
    /// <param name="count">Amount to increment.</param>
    /// <returns>The new cumulative value.</returns>
    Task<long> IncrementAsync(string tenantId, string counterType, string dimensionKey, string granularity, long count = 1);

    /// <summary>
    /// Scan all pending (not yet drained) counters into a list of batch records.
    /// </summary>
    Task<IReadOnlyList<CounterBatch>> ScanPendingBatchAsync(CancellationToken ct);

    /// <summary>
    /// Remove keys that have been successfully drained from Redis.
    /// </summary>
    Task RemoveDrainedKeysAsync(IEnumerable<string> drainedKeys, CancellationToken ct);
}

/// <summary>
/// Represents a batch of counters to be inserted into the relational database.
/// </summary>
public record CounterBatch(
    string CounterType,
    string Granularity,
    string DimensionKey,
    long Value,
    DateTimeOffset RecordedAt);
