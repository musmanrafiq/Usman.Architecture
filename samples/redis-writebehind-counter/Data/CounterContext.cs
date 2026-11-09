using Microsoft.EntityFrameworkCore;

namespace RedisWriteBehindCounter.Data;

/// <summary>
/// Entity Framework Core DbContext for the aggregated counter table.
/// </summary>
public class CounterContext : DbContext
{
    public CounterContext(DbContextOptions<CounterContext> options)
        : base(options)
    {
    }

    public DbSet<CounterAggregated> CounterAggregateds { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CounterAggregated>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CounterType, e.Granularity, e.DimensionKey, e.RecordedAt })
                .IsUnique()
                .HasDatabaseName("IX_CounterAggregated_Type_Granularity_Dimension_Recorded");

            entity.Property(e => e.CounterType).HasMaxLength(50);
            entity.Property(e => e.Granularity).HasMaxLength(20);
            entity.Property(e => e.DimensionKey).HasMaxLength(200);
        });
    }
}

/// <summary>
/// Aggregated counter row stored in SQL.
///
/// This is the "sink" for the write-behind cache pattern.
/// Values are cumulative and time-based.
/// </summary>
public class CounterAggregated
{
    public long Id { get; set; }

    public string CounterType { get; set; } = string.Empty;

    public string Granularity { get; set; } = string.Empty;

    public string DimensionKey { get; set; } = string.Empty;

    public long Value { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}
