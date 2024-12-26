using Application.Request;
using Application.Response;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

public interface IRequestPipeline
{
    Task<HttpResponse> ExecuteAsync(HttpRequest httpRequest);
}

public class TypedRequestPipeline : IRequestPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICollection<Type> _plugins;
    private Func<HttpRequest, Task<HttpResponse>> DefaultHandler { get; init; }
        = _ => Task.FromResult(new HttpResponse(HttpResponseStatusCode.OK, new HttpBody("text/plain", "Hello, World!")));
    
    public TypedRequestPipeline(IServiceProvider serviceProvider)
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
        var requestPipelineContext = new RequestPipelineContext(httpRequest);
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