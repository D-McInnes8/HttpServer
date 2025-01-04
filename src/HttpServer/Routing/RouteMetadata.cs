using HttpServer.Request;
using HttpServer.Response;

namespace HttpServer.Routing;

public class RouteMetadata
{
    public required Func<HttpRequest, HttpResponse> Handler { get; init; }
    public string? Pipeline { get; init; }
}