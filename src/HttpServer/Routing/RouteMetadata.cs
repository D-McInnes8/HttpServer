using HttpServer.Pipeline;
using HttpServer.Response;

namespace HttpServer.Routing;

/// <summary>
/// 
/// </summary>
public class RouteMetadata
{
    /// <summary>
    /// Represents the handler for the route.
    /// </summary>
    public required Func<RequestPipelineContext, HttpResponse> Handler { get; init; }
    
    /// <summary>
    /// The name of the pipeline to use for the route. if NULL, the global request pipeline will call the handler directly.
    /// </summary>
    public string? Pipeline { get; init; }
    
    /// <summary>
    /// Additional metadata for the route.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}