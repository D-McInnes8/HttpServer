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
        return _pipelineRegistry.GlobalPipeline.ExecuteAsync(ctx).GetAwaiter().GetResult();

        /*foreach (var requestPipeline in _pipelineRegistry)
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
        }*/

        //return new HttpResponse(HttpResponseStatusCode.NotFound);
    }
}