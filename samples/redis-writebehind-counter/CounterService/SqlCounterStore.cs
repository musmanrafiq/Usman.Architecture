using Microsoft.EntityFrameworkCore;
using RedisWriteBehindCounter.Data;

namespace RedisWriteBehindCounter.CounterService;

/// <summary>
/// SQL-backed bulk counter store.
/// </summary>
public interface ISqlCounterStore
{
    /// <summary>
    /// Bulk-insert a batch of counter rows into the database.
    /// </summary>
    Task<int> BulkInsertAsync(IEnumerable<CounterBatch> batches, CancellationToken ct);
}

public class SqlCounterStore : ISqlCounterStore
{
    private readonly CounterContext _context;
    private readonly ILogger<SqlCounterStore> _logger;

    public SqlCounterStore(CounterContext context, ILogger<SqlCounterStore> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> BulkInsertAsync(IEnumerable<CounterBatch> batches, CancellationToken ct)
    {
        var list = batches as CounterBatch[] ?? batches.ToList();
        if (list.Count == 0) return 0;

        var entities = list.Select(b => new CounterAggregated
        {
            CounterType = b.CounterType,
            Granularity = b.Granularity,
            DimensionKey = b.DimensionKey,
            Value = b.Value,
            RecordedAt = b.RecordedAt
        }).ToList();

        await _context.CounterAggregateds.AddRangeAsync(entities, ct);
        var affected = await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Bulk inserted {Count} counter rows into SQL", affected);
        return affected;
    }
}
