using HttpServer.Routing;

namespace HttpServer.Pipeline.StaticFiles;

/// <summary>
/// Router for static files
/// </summary>
public class StaticFileRouter : IRouter
{
    /// <inheritdoc />
    public Task<RouterResult> RouteAsync(RequestPipelineContext ctx)
    {
        var options = ctx.GetOptions<StaticFilePipelineOptions>();
        if (options is null)
        {
            return Task.FromResult(RouterResult.NotFound);
        }
        
        var requestPath = ctx.Request.Route;
        foreach (var route in options.Routes)
        {
            if (!route.IsDirectory && requestPath == route.VirtualPath)
            {
                ctx.SetData(route);
                return Task.FromResult(RouterResult.Success);
            }
            
            //if (route.IsDirectory && requestPath.StartsWithSegments(route.VirtualPath))
            var path = Path.Combine(Directory.GetCurrentDirectory() + "/" + route.PhysicalPath, requestPath[(route.VirtualPath.Length + 1)..]);
            if (route.IsDirectory && requestPath.StartsWith(route.VirtualPath)
                && File.Exists(Path.Combine(route.PhysicalPath, path)))
            {
                var newRoute = new StaticFileRoute(route.PhysicalPath, path, false);
                ctx.SetData(newRoute);
                return Task.FromResult(RouterResult.Success);
            }
        }
        
        return Task.FromResult(RouterResult.NotFound);
    }
}