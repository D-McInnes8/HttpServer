using HttpServer.Pipeline;
using HttpServer.Response;

namespace HttpServer.Routing;

public class RouteMetadata
{
    public required Func<RequestPipelineContext, HttpResponse> Handler { get; init; }
    public string? Pipeline { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}