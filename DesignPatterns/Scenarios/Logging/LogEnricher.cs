namespace Scenarios.Logging;

/// <summary>
/// Chain of Responsibility base — each enricher adds properties to the log entry
/// and then passes control to the next enricher in the pipeline.
/// </summary>
public abstract class LogEnricher
{
    private LogEnricher? _next;

    public LogEnricher SetNext(LogEnricher next) { _next = next; return next; }

    public virtual void Enrich(LogEntry entry)
    {
        _next?.Enrich(entry);
    }
}
