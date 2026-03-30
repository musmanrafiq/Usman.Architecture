namespace Scenarios.Logging;

public class LogEntry
{
    public string Level { get; set; } = "INFO";
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Properties { get; } = [];

    public override string ToString() =>
        $"[{Timestamp:HH:mm:ss.fff}] [{Level,-5}] {Message} | " +
        string.Join(", ", Properties.Select(p => $"{p.Key}={p.Value}"));
}
