namespace Scenarios.Logging;

public class EnvironmentEnricher : LogEnricher
{
    public override void Enrich(LogEntry entry)
    {
        entry.Properties["Environment"] = "Production";
        base.Enrich(entry);
    }
}
