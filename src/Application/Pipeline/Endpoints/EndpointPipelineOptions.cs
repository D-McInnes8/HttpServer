using Application.Request;
using Application.Response;
using Application.Routing;

namespace Application.Pipeline.Endpoints;

/// <summary>
/// 
/// </summary>
public class EndpointPipelineOptions : RequestPipelineBuilderOptions
{
    private readonly IRouteRegistry _routeRegistry;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="routeRegistry"></param>
    public EndpointPipelineOptions(IServiceProvider services, IRouteRegistry routeRegistry) : base(services)
    {
        _routeRegistry = routeRegistry;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    public void MapRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        _routeRegistry.AddRoute(method, path, handler);
    }
}

/// <summary>
/// 
/// </summary>
public static class EndpointHttpServerExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpServer"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IHttpServer AddEndpointPipeline(this IHttpServer httpServer, Action<EndpointPipelineOptions> configure)
    {
        return httpServer.AddPipeline<EndpointPipelineOptions>(options =>
        {
            options.UseRouter<EndpointRouter>();
            options.UseRequestHandler<EndpointRequestHandler>();
            configure(options);
        });
    }
}