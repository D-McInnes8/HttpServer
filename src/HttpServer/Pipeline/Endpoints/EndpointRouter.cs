using HttpServer.Routing;

namespace HttpServer.Pipeline.Endpoints;

/// <summary>
/// Router that matches the request to an endpoint in the route registry.
/// </summary>
public class EndpointRouter : IRouter
{
    private readonly IRouteRegistry _routeRegistry;

    /// <summary>
    /// Creates a new instance of <see cref="EndpointRouter"/>.
    /// </summary>
    /// <param name="routeRegistry"></param>
    public EndpointRouter(IRouteRegistry routeRegistry)
    {
        _routeRegistry = routeRegistry;
    }

    /// <inheritdoc />
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