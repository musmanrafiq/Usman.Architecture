namespace Scenarios.Logging;

/// <summary>
/// Builds and owns the enricher pipeline.
/// Consumers call Log() — they never interact with enrichers directly.
/// </summary>
public class StructuredLogger
{
    private readonly LogEnricher _pipeline;

    public StructuredLogger()
    {
        var machine = new MachineNameEnricher();
        var environment = new EnvironmentEnricher();
        var correlation = new CorrelationIdEnricher();
        machine.SetNext(environment).SetNext(correlation);
        _pipeline = machine;
    }

    public void Log(string level, string message)
    {
        var entry = new LogEntry { Level = level, Message = message };
        _pipeline.Enrich(entry);
        Console.WriteLine(entry);
    }
}
