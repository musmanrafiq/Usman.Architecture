namespace Scenarios.Logging;

public class CorrelationIdEnricher : LogEnricher
{
    public override void Enrich(LogEntry entry)
    {
        entry.Properties["CorrelationId"] = Guid.NewGuid().ToString("N")[..8];
        base.Enrich(entry);
    }
}
