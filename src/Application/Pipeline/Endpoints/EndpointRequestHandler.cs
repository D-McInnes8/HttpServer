using Application.Response;

namespace Application.Pipeline.Endpoints;

public class EndpointRequestHandler : IRequestHandler
{
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        if (ctx.Route is not null)
        {
            return Task.FromResult(ctx.Route.Handler(ctx.Request));
        }
        
        return Task.FromResult(HttpResponse.NotFound());
    }
}