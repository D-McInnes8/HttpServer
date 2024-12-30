using Application.Routing;

namespace Application.Pipeline.Endpoints;

public class EndpointRouter : IRouter
{
    private readonly IRouteRegistry _routeRegistry;

    public EndpointRouter(IRouteRegistry routeRegistry)
    {
        _routeRegistry = routeRegistry;
    }

    public Task<RouterResult> RouteAsync(RequestPipelineContext ctx)
    {
        var route = _routeRegistry.MatchRoute(ctx.Request);

        if (route is null)
        {
            return Task.FromResult(RouterResult.NotFound);
        }

        ctx.SetData(route);
        return Task.FromResult(RouterResult.Success);
    }
}