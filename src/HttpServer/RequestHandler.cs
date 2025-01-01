using HttpServer.Pipeline;
using HttpServer.Pipeline.Registry;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HttpServer;

/// <summary>
/// The request handler for the web server.
/// </summary>
public class RequestHandler
{
    private readonly IReadOnlyPipelineRegistry _pipelineRegistry;
    
    /// <summary>
    /// Creates a new <see cref="RequestHandler"/> with the specified pipeline registry.
    /// </summary>
    /// <param name="pipelineRegistry"></param>
    public RequestHandler(IReadOnlyPipelineRegistry pipelineRegistry)
    {
        _pipelineRegistry = pipelineRegistry;
    }

    /// <summary>
    /// Handles the HTTP request.
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public HttpResponse HandleRequest(HttpRequest httpRequest, IServiceProvider serviceProvider)
    {
        var ctx = new RequestPipelineContext(httpRequest, serviceProvider, _pipelineRegistry.GlobalPipeline.Options);
        
        foreach (var requestPipeline in _pipelineRegistry)
        {
            if (ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, requestPipeline.Options.Router) is not
                IRouter router)
            {
                continue;
            }
            
            ctx.Options = requestPipeline.Options;
            var routingResult = router.RouteAsync(ctx).GetAwaiter().GetResult();
            if (routingResult == RouterResult.Success)
            {
                return requestPipeline.ExecuteAsync(ctx).GetAwaiter().GetResult();
            }
        }
        
        return new HttpResponse(HttpResponseStatusCode.NotFound);
    }
}

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
            
            var routingResult = await router.RouteAsync(ctx);
            if (routingResult == RouterResult.Success)
            {
                return await requestPipeline.ExecuteAsync(ctx);
            }
        }
        
        return new HttpResponse(HttpResponseStatusCode.NotFound);
    }
}