namespace Creational.Builder;

public sealed class HttpRequestBuilder
{
    private string _method = "GET";
    private string _url = string.Empty;
    private readonly Dictionary<string, string> _headers = [];
    private string? _body;
    private int _timeout = 30;

    public HttpRequestBuilder WithMethod(string method) { _method = method; return this; }
    public HttpRequestBuilder WithUrl(string url) { _url = url; return this; }
    public HttpRequestBuilder WithHeader(string key, string value) { _headers[key] = value; return this; }
    public HttpRequestBuilder WithBody(string body) { _body = body; return this; }
    public HttpRequestBuilder WithTimeout(int seconds) { _timeout = seconds; return this; }
    public HttpRequestBuilder WithBearerToken(string token) => WithHeader("Authorization", $"Bearer {token}");

    public HttpRequest Build()
    {
        if (string.IsNullOrWhiteSpace(_url))
            throw new InvalidOperationException("URL is required.");

        return new HttpRequest
        {
            Method = _method,
            Url = _url,
            Headers = new Dictionary<string, string>(_headers),
            Body = _body,
            TimeoutSeconds = _timeout
        };
    }
}
