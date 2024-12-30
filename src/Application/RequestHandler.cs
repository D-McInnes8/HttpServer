using Application.Pipeline;
using Application.Pipeline.Registry;
using Application.Request;
using Application.Response;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public class RequestHandler
{
    private readonly List<Route> _routes;
    private readonly IReadOnlyPipelineRegistry _pipelineRegistry;
    
    public RequestHandler(IReadOnlyPipelineRegistry pipelineRegistry)
    {
        _pipelineRegistry = pipelineRegistry;
        _routes = new List<Route>();
    }

    public RequestHandler(ICollection<Route> routes, IReadOnlyPipelineRegistry pipelineRegistry)
    {
        _pipelineRegistry = pipelineRegistry;
        _routes = routes.ToList();
    }

    public void AddRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        _routes.Add(new Route(method, path, handler));
    }

    public HttpResponse HandleRequest(HttpRequest httpRequest, IServiceProvider serviceProvider)
    {
        var ctx = new RequestPipelineContext(httpRequest, serviceProvider);
        var globalRequestPipeline = _pipelineRegistry.GlobalPipeline;
        
        foreach (var requestPipeline in _pipelineRegistry)
        {
            if (ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, requestPipeline.Options.Router) is not
                IRouter router)
            {
                continue;
            }
            
            var routingResult = router.RouteAsync(ctx).GetAwaiter().GetResult();
            if (routingResult == RouterResult.Success)
            {
                return requestPipeline.ExecuteAsync(ctx).GetAwaiter().GetResult();
            }
        }
        
        return new HttpResponse(HttpResponseStatusCode.NotFound);
        
        /*var requestPipeline = new RequestPipeline(serviceProvider);
        requestPipeline.AddPlugin<RoutingPlugin>();
        try
        {
            return requestPipeline.ExecuteAsync(httpRequest).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return new HttpResponse(HttpResponseStatusCode.InternalServerError);
        }*/


        /*foreach (var route in _routes)
        {
            if (route.IsMatch(httpRequest.Method, httpRequest.Route))
            {
                try
                {
                    //var response = requestPipeline.ExecuteAsync(route.Handler);
                    return route.Handler(httpRequest);
                }
                catch (Exception ex)
                {
                    return new HttpResponse(HttpResponseStatusCode.InternalServerError, new HttpBody
                    {
                        ContentType = "text/plain",
                        Content = ex.Message
                    });
                }
            }
        }

        return new HttpResponse(HttpResponseStatusCode.NotFound);*/
    }
}

/// <summary>
/// 
/// </summary>
public class GlobalPipelineRequestHandler : IRequestHandler
{
    private readonly IReadOnlyPipelineRegistry _pipelineRegistry;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pipelineRegistry"></param>
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