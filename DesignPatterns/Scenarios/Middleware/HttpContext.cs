namespace Scenarios.Middleware;

public class HttpContext
{
    public string Method { get; init; } = "GET";
    public string Path { get; init; } = "/";
    public Dictionary<string, string> Headers { get; } = [];
    public int StatusCode { get; set; } = 200;
    public bool Aborted { get; set; }
}
