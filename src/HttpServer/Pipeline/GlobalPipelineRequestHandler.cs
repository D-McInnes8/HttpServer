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

    /// <summary>
    /// Creates a new <see cref="GlobalPipelineRequestHandler"/> with the specified pipeline registry.
    /// </summary>
    /// <param name="pipelineRegistry">The <see cref="IReadOnlyPipelineRegistry"/>.</param>
    public GlobalPipelineRequestHandler(IReadOnlyPipelineRegistry pipelineRegistry)
    {
        _pipelineRegistry = pipelineRegistry;
    }

    /// <inheritdoc />
    public async Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        foreach (var requestPipeline in _pipelineRegistry)
        {
            if (ActivatorUtilities.GetServiceOrCreateInstance(ctx.Services, requestPipeline.Options.Router)
                is not IRouter router)
            {
                continue;
            }
            
            ctx.Options = requestPipeline.Options;
            var routingResult = await router.RouteAsync(ctx);
            if (routingResult == RouterResult.Success)
            {
                var result = await requestPipeline.ExecuteAsync(ctx);
                ctx.Options = _pipelineRegistry.GlobalPipeline.Options;
                return result;
            }
        }
        
        ctx.Options = _pipelineRegistry.GlobalPipeline.Options;
        return new HttpResponse(HttpResponseStatusCode.NotFound);
    }
}