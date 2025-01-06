using HttpServer.Response;

namespace HttpServer.Pipeline;

/// <summary>
/// A default request handler that will be used if no other request handler is found. This handler will return a 404 Not Found response.
/// </summary>
public class DefaultRequestHandler : IRequestHandler
{
    /// <inheritdoc />
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        if (ctx.Route is not null)
        {
            return Task.FromResult(ctx.Route.Handler(ctx));
        }
        
        return Task.FromResult(HttpResponse.NotFound());
    }
}