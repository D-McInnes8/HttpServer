using Application.Response;
using Application.Routing;

namespace Application.Pipeline.Endpoints;

public class EndpointRequestHandler : IRequestHandler
{
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        var route = ctx.GetData<Route>();
        if (route is not null)
        {
            return Task.FromResult(route.Handler(ctx.Request));
        }
        
        return Task.FromResult(HttpResponse.NotFound());
    }
}