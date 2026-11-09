namespace Scenarios.Logging;

public class MachineNameEnricher : LogEnricher
{
    public override void Enrich(LogEntry entry)
    {
        entry.Properties["MachineName"] = Environment.MachineName;
        base.Enrich(entry);
    }
}
