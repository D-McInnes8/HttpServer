using Application.Request;
using Application.Response;
using Application.Routing;

namespace Application.Pipeline.Endpoints;

/// <summary>
/// Pipeline options used to configure the endpoint pipeline.
/// </summary>
public class EndpointPipelineOptions : RequestPipelineBuilderOptions
{
    private readonly IRouteRegistry _routeRegistry;

    /// <summary>
    /// Creates a new instance of <see cref="EndpointPipelineOptions"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> associated with this web server.</param>
    /// <param name="routeRegistry">The <see cref="IRouteRegistry"/> used to retrieve endpoint routing information.</param>
    public EndpointPipelineOptions(IServiceProvider services, IRouteRegistry routeRegistry) : base(services)
    {
        _routeRegistry = routeRegistry;
    }

    /// <summary>
    /// Map a route to the specified handler.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMethod"/> to be used for this endpoint.</param>
    /// <param name="path">The Uri/Path to be used for this endpoint.</param>
    /// <param name="handler">The handler that will be invoked by the request pipeline.</param>
    public void MapRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        _routeRegistry.AddRoute(method, path, handler);
    }
}

/// <summary>
/// Extension methods for configuring the endpoint pipeline.
/// </summary>
public static class EndpointHttpServerExtensions
{
    /// <summary>
    /// Adds an endpoint pipeline to the <see cref="IHttpWebServer"/>.
    /// </summary>
    /// <param name="httpWebServer">The <see cref="IHttpWebServer"/> to add the pipeline to.</param>
    /// <param name="configure">Configure the request pipeline.</param>
    /// <returns>The <see cref="IHttpWebServer"/> instance, for chaining methods.</returns>
    public static IHttpWebServer AddEndpointPipeline(this IHttpWebServer httpWebServer, Action<EndpointPipelineOptions> configure)
    {
        return httpWebServer.AddPipeline<EndpointPipelineOptions>(options =>
        {
            options.UseRouter<EndpointRouter>();
            options.UseRequestHandler<EndpointRequestHandler>();
            configure(options);
        });
    }
}