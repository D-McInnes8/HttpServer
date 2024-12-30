using Application.Request;
using Application.Response;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

public interface IRequestPipeline3
{
    public string Name { get; }
    
    public RequestPipelineBuilderOptions Options { get; }
    
    Task<HttpResponse> ExecuteAsync(HttpRequest httpRequest);
}

/// <summary>
/// 
/// </summary>
public interface IRequestPipeline
{
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }

    /*/// <summary>
    /// 
    /// </summary>
    public static abstract IReadOnlyCollection<Type> DefaultPlugins { get; }*/
    
    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyCollection<Type> Plugins { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public Task<HttpResponse> RouteRequest(RequestPipelineContext ctx,  Func<RequestPipelineContext,Task<HttpResponse>> next);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public Task<HttpResponse> HandleRequest(RequestPipelineContext ctx);
}

public class Testipeline : IRequestPipeline
{
    public string Name { get; }

    public static IReadOnlyCollection<Type> DefaultPlugins { get; } = [];
    
    public IReadOnlyCollection<Type> Plugins { get; }
    
    public Task<HttpResponse> RouteRequest(RequestPipelineContext ctx, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponse> HandleRequest(RequestPipelineContext ctx)
    {
        throw new NotImplementedException();
    }
}

public abstract class HttpRequestPipeline
{
    private readonly List<Type> _plugins;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plugins"></param>
    /// <param name="serviceProvider"></param>
    protected HttpRequestPipeline(List<Type> plugins, IServiceProvider serviceProvider)
    {
        _plugins = plugins;
        _serviceProvider = serviceProvider;
    }

    public async Task<HttpResponse> ExecuteAsync(HttpRequest httpRequest)
    {
        var requestPipelineContext = new RequestPipelineContext(httpRequest, _serviceProvider);
        var pipeline = _plugins
            .Aggregate(
                seed: HandleRequest,
                func: (next, type) => ctx
                    => ((IRequestPipelinePlugin)_serviceProvider.GetRequiredService(type)).InvokeAsync(ctx, next));
        
        return await RouteRequest(requestPipelineContext, pipeline);
    }

    public void AddPlugin<T>() where T : IRequestPipelinePlugin
    {
        _plugins.Add(typeof(T));
    }

    protected abstract Task<HttpResponse> RouteRequest(RequestPipelineContext ctx,  Func<RequestPipelineContext,Task<HttpResponse>> next);
    protected abstract Task<HttpResponse> HandleRequest(RequestPipelineContext ctx);
}

public class RequestPipeline2 : HttpRequestPipeline
{
    private readonly IRouteRegistry _routeRegistry;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="plugins"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="routeRegistry"></param>
    public RequestPipeline2(
        List<Type> plugins,
        IServiceProvider serviceProvider,
        IRouteRegistry routeRegistry) : base(plugins, serviceProvider)
    {
        _routeRegistry = routeRegistry;
    }
    
    protected override Task<HttpResponse> RouteRequest(RequestPipelineContext ctx, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        ctx.Route ??= _routeRegistry.MatchRoute(ctx.Request);
        if (ctx.Route is not null)
        {
            return next(ctx);
        }
        
        return Task.FromResult(new HttpResponse(HttpResponseStatusCode.NotFound));
    }

    protected override Task<HttpResponse> HandleRequest(RequestPipelineContext ctx)
    {
        if (ctx.Route is not null)
        {
            return Task.FromResult(ctx.Route.Handler(ctx.Request));
        }
        
        return Task.FromResult(new HttpResponse(HttpResponseStatusCode.OK, new HttpBody("text/plain", "Hello, World!")));
    }
}


public class RequestPipeline : IRequestPipeline3
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICollection<Type> _plugins;

    public string Name => Options.Name;
    public RequestPipelineBuilderOptions Options { get; }
    
    private Func<HttpRequest, Task<HttpResponse>> DefaultHandler { get; init; }
        = _ => Task.FromResult(new HttpResponse(HttpResponseStatusCode.OK, new HttpBody("text/plain", "Hello, World!")));
    
    public RequestPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _plugins = new List<Type>();
    }

    public RequestPipeline(RequestPipelineBuilderOptions options)
    {
        _serviceProvider = null!;
        _plugins = new List<Type>();
        Options = options;
    }

    public void AddPlugin<T>() where T : IRequestPipelinePlugin
    {
        _plugins.Add(typeof(T));
    }

    public async Task<HttpResponse> ExecuteAsync(HttpRequest httpRequest)
    {
        var requestPipelineContext = new RequestPipelineContext(httpRequest, _serviceProvider);
        var pipeline = _plugins
            .Aggregate(
                seed: ExecuteRequestHandler,
                func: (next, type) => ctx
                    => ((IRequestPipelinePlugin)_serviceProvider.GetRequiredService(type)).InvokeAsync(ctx, next));
        
        return await pipeline(requestPipelineContext);
    }
    
    private Task<HttpResponse> ExecuteRequestHandler(RequestPipelineContext context)
    {
        if (context.Route is not null)
        {
            return Task.FromResult(context.Route.Handler(context.Request));
        }
        
        return DefaultHandler(context.Request);
    }
}