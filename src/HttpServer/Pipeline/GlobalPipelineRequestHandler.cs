using System.Diagnostics;
using HttpServer.Pipeline.Registry;
using HttpServer.Response;
using HttpServer.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HttpServer.Pipeline;

/// <summary>
/// The global pipeline request handler. This handler will execute all pipelines in the pipeline registry.
/// </summary>
public class GlobalPipelineRequestHandler : IRequestHandler
{
    private readonly IReadOnlyPipelineRegistry _pipelineRegistry;
    private readonly IRouter _router;
    private readonly IHttpRouter _router2;

    /// <summary>
    /// Creates a new <see cref="GlobalPipelineRequestHandler"/> with the specified pipeline registry.
    /// </summary>
    /// <param name="pipelineRegistry">The <see cref="IReadOnlyPipelineRegistry"/>.</param>
    /// <param name="router2">The <see cref="IHttpRouter"/> used to route requests to a specific endpoint.</param>
    /// <param name="router">The <see cref="IRouter"/> used to route requests to a specific endpoint.</param>
    public GlobalPipelineRequestHandler(IReadOnlyPipelineRegistry pipelineRegistry, IHttpRouter router2, IRouter router)
    {
        _pipelineRegistry = pipelineRegistry;
        _router2 = router2;
        _router = router;
    }

    /// <inheritdoc />
    public async Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        var routingResult = _router.Match(new Route(ctx.Request.Route, ctx.Request.Method));
        if (routingResult.Result == RouterResult.Success)
        {
            ctx.Route = routingResult.Value;
            ctx.RouteParameters = new RouteParameters(routingResult.Parameters);
            
            // Route is configured to use a request pipeline.
            if (routingResult.Value?.Pipeline is not null
                && _pipelineRegistry.TryGetPipeline(routingResult.Value.Pipeline, out var pipeline))
            {
                ctx.Options = pipeline.Options;
                try
                {
                    var result = await pipeline.ExecuteAsync(ctx);
                    return result;
                }
                finally
                {
                    ctx.Options = _pipelineRegistry.GlobalPipeline.Options;
                }
            }

            if (ctx.Route?.Pipeline is not null)
            {
                throw new InvalidOperationException($"The pipeline '{routingResult.Value?.Pipeline}' was not found.");
            }
            
            // Route has no pipeline, execute the handler directly.
            if (ctx.Route?.Handler is not null)
            {
                return ctx.Route.Handler(ctx);
            }
            
            Debug.Fail("Route matched but has no handler or pipeline.");
        }

        if (routingResult.Result == RouterResult.MethodNotAllowed)
        {
            return HttpResponse.MethodNotAllowed();
        }

        return new HttpResponse(HttpResponseStatusCode.NotFound);
    }
}