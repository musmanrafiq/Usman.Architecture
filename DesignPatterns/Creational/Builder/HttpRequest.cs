namespace Creational.Builder;

public sealed class HttpRequest
{
    public string Method { get; init; } = "GET";
    public string Url { get; init; } = string.Empty;
    public Dictionary<string, string> Headers { get; init; } = [];
    public string? Body { get; init; }
    public int TimeoutSeconds { get; init; } = 30;

    public override string ToString() =>
        $"{Method} {Url} | Headers: {Headers.Count} | Timeout: {TimeoutSeconds}s | Body: {Body ?? "(none)"}";
}
