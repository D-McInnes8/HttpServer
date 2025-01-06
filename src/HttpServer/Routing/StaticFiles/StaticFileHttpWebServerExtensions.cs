using HttpServer.Request;

namespace HttpServer.Routing.StaticFiles;

/// <summary>
/// Extension methods for configuring the <see cref="IHttpWebServer"/> to serve static files.
/// </summary>
public static class StaticFileHttpWebServerExtensions
{
    public static IHttpWebServer ServeFile(
        this IHttpWebServer httpWebServer,
        string virtualPath,
        string physicalPath,
        string? pipelineName = null)
    {
        var route = new RouteMetadata
        {
            Handler = StaticFileRequestHandler.HandleIndividualFile,
            Pipeline = pipelineName,
        };
        route.Metadata.Add("VirtualPath", virtualPath);
        route.Metadata.Add("PhysicalPath", physicalPath);
        httpWebServer.MapRoute(HttpRequestMethod.GET, virtualPath, route);
        return httpWebServer;
    }
    
    /// <summary>
    /// Configures the <see cref="IHttpWebServer"/> to serve static files from the specified directory.
    /// </summary>
    /// <param name="httpWebServer"></param>
    /// <param name="virtualPath"></param>
    /// <param name="physicalPath"></param>
    /// <param name="pipelineName"></param>
    /// <returns></returns>
    public static IHttpWebServer ServeDirectory(
        this IHttpWebServer httpWebServer,
        string virtualPath,
        string physicalPath,
        string? pipelineName = null)
    {
        //httpWebServer.MapRoute(HttpRequestMethod.GET, virtualPath, new StaticFileRouteMetadata(physicalPath));
        var route = new RouteMetadata
        {
            Handler = StaticFileRequestHandler.HandleDirectory,
            Pipeline = pipelineName,
        };
        route.Metadata.Add("VirtualPath", virtualPath);
        route.Metadata.Add("PhysicalPath", physicalPath);
        httpWebServer.MapRoute(HttpRequestMethod.GET, $"{{*}}", route);
        return httpWebServer;
    }
}