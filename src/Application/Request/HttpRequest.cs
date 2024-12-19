namespace Application.Request;

public class HttpRequest
{
    public required HttpRequestMethod Method { get; init; }
    public string HttpVersion { get; init; }
    public required string Path { get; init; }
    public bool HasBody => Body is not null;
    public string? Body { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
}