using HttpServer.Pipeline;
using HttpServer.Request;
using HttpServer.Response;

namespace HttpServer.Routing;

/// <summary>
/// Extension helper methods for creating <see cref="IHttpWebServer"/> routes.
/// </summary>
public static class RouterHttpWebServerExtensions
{
    /// <summary>
    /// Maps a route to the HTTP method and the specified path.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapGet(
        this IHttpWebServer httpWebServer,
        string path,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        return httpWebServer.MapRoute(HttpRequestMethod.GET, path, handler);
    }
    
    /// <summary>
    /// Maps a HTTP GET route to the specified path and pipeline.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="pipelineName"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapGet(
        this IHttpWebServer httpWebServer,
        string path,
        string pipelineName,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        var metadata = new RouteMetadata
        {
            Handler = handler,
            Pipeline = pipelineName,
        };
        httpWebServer.MapRoute(HttpRequestMethod.GET, path, metadata);
        return httpWebServer;
    }
    
    /// <summary>
    /// Maps a HTTP POST route to the specified path.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapPost(
        this IHttpWebServer httpWebServer,
        string path,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        return httpWebServer.MapRoute(HttpRequestMethod.POST, path, handler);
    }
    
    /// <summary>
    /// Maps a HTTP POST route to the specified path and pipeline.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="pipelineName"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapPost(
        this IHttpWebServer httpWebServer,
        string path,
        string pipelineName,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        var metadata = new RouteMetadata
        {
            Handler = handler,
            Pipeline = pipelineName,
        };
        httpWebServer.MapRoute(HttpRequestMethod.POST, path, metadata);
        return httpWebServer;
    }
    
    /// <summary>
    /// Maps a HTTP PUT route to the specified path.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapPut(
        this IHttpWebServer httpWebServer,
        string path,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        return httpWebServer.MapRoute(HttpRequestMethod.PUT, path, handler);
    }
    
    /// <summary>
    /// Maps a HTTP PUT route to the specified path and pipeline.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="pipelineName"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapPut(
        this IHttpWebServer httpWebServer,
        string path,
        string pipelineName,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        var metadata = new RouteMetadata
        {
            Handler = handler,
            Pipeline = pipelineName,
        };
        httpWebServer.MapRoute(HttpRequestMethod.PUT, path, metadata);
        return httpWebServer;
    }
    
    /// <summary>
    /// Maps a HTTP DELETE route to the specified path.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapDelete(
        this IHttpWebServer httpWebServer,
        string path,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        return httpWebServer.MapRoute(HttpRequestMethod.DELETE, path, handler);
    }
    
    /// <summary>
    /// Maps a HTTP DELETE route to the specified path and pipeline.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="pipelineName"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapDelete(
        this IHttpWebServer httpWebServer,
        string path,
        string pipelineName,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        var metadata = new RouteMetadata
        {
            Handler = handler,
            Pipeline = pipelineName,
        };
        httpWebServer.MapRoute(HttpRequestMethod.DELETE, path, metadata);
        return httpWebServer;
    }
    
    /// <summary>
    /// Maps a HTTP PATCH route to the specified path.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapPatch(
        this IHttpWebServer httpWebServer,
        string path,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        return httpWebServer.MapRoute(HttpRequestMethod.PATCH, path, handler);
    }
    
    /// <summary>
    /// Maps a HTTP PATCH route to the specified path and pipeline.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="path"></param>
    /// <param name="pipelineName"></param>
    /// <param name="handler"></param>
    /// <returns>The <see cref="IHttpWebServer"/> instance for chaining methods.</returns>
    public static IHttpWebServer MapPatch(
        this IHttpWebServer httpWebServer,
        string path,
        string pipelineName,
        Func<RequestPipelineContext, HttpResponse> handler)
    {
        var metadata = new RouteMetadata
        {
            Handler = handler,
            Pipeline = pipelineName,
        };
        httpWebServer.MapRoute(HttpRequestMethod.PATCH, path, metadata);
        return httpWebServer;
    }
}