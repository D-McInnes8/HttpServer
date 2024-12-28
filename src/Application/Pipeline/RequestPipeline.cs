using Application.Request;
using Application.Response;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

public interface IRequestPipeline
{
    Task<HttpResponse> ExecuteAsync(HttpRequest httpRequest);
    
    
}

public interface IRequestPipeline2
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public Task<HttpResponse> ExecuteAsync(HttpRequest httpRequest);
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void AddPlugin<T>() where T : IRequestPipelinePlugin;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public Task<HttpResponse> RouteRequest(RequestPipelineContext ctx);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public Task<HttpResponse> HandleRequest(RequestPipelineContext ctx);
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

        //Func<RequestPipelineContext, Task<HttpResponse>> withRouter = (ctx) => RouteRequest(ctx, (innerCtx) => pipeline(innerCtx));
        //return await pipeline(requestPipelineContext);
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


public class RequestPipeline : IRequestPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICollection<Type> _plugins;
    private Func<HttpRequest, Task<HttpResponse>> DefaultHandler { get; init; }
        = _ => Task.FromResult(new HttpResponse(HttpResponseStatusCode.OK, new HttpBody("text/plain", "Hello, World!")));
    
    public RequestPipeline(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _plugins = new List<Type>();
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