using HttpServer.Pipeline;
using HttpServer.Response;

namespace HttpServer.Routing;

public class RoutingPlugin : IRequestPipelinePlugin
{
    private readonly IRouteRegistry _routeRegistry;

    public RoutingPlugin(IRouteRegistry routeRegistry)
    {
        _routeRegistry = routeRegistry;
    }

    public Task<HttpResponse> InvokeAsync(RequestPipelineContext context, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        /*context.Route ??= _routeRegistry.MatchRoute(context.Request);
        
        if (context.Route is not null)
        {
            return next(context);
        }*/
        
        return Task.FromResult(new HttpResponse(HttpResponseStatusCode.NotFound));
    }
}