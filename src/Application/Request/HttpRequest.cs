using System.Diagnostics.CodeAnalysis;
using Application.Request.Parser;

namespace Application.Request;

public class HttpRequest
{
    public required HttpRequestMethod Method { get; init; }
    public string HttpVersion { get; init; } = "HTTP/1.1";
    public required string Path { get; init; }
    public required string Route { get; init; }
    public bool HasBody => Body is not null;
    public string? Body { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
    public string? ContentType { get; init; }
    public Dictionary<string, string> QueryParameters { get; init; }

    [SetsRequiredMembers]
    public HttpRequest(HttpRequestMethod method, string path)
    {
        Method = method;
        Path = path;
        Headers = new Dictionary<string, string>();
        QueryParameters = new Dictionary<string, string>();

        var (route, parameters) = HttpRequestParser.ParsePath(Path);
        Route = route;
        QueryParameters = parameters;
    }
}