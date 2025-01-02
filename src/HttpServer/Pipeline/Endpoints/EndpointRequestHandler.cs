using HttpServer.Response;
using HttpServer.Routing;

namespace HttpServer.Pipeline.Endpoints;

/// <summary>
/// The request handler for the endpoint request pipeline. This handler is responsible for invoking the appropriate
/// request handler for the matched route.
/// </summary>
public class EndpointRequestHandler : IRequestHandler
{
    /// <inheritdoc />
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        var route = ctx.GetData<RouteOld>();
        if (route is not null)
        {
            return Task.FromResult(route.Handler(ctx.Request));
        }
        
        return Task.FromResult(HttpResponse.NotFound());
    }
}