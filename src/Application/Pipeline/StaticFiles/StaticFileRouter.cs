using Application.Routing;

namespace Application.Pipeline.StaticFiles;

public class StaticFileRouter : IRouter
{
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
            if (route.IsDirectory && requestPath.StartsWith(route.VirtualPath))
            {
                ctx.SetData(route);
                return Task.FromResult(RouterResult.Success);
            }
        }
        
        return Task.FromResult(RouterResult.NotFound);
    }
}