using Application.Response;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

public interface IRequestPipeline
{
    Task<HttpResponse> ExecuteAsync(Func<RequestPipelineContext, Task<HttpResponse>> requestHandler);
}

public class RequestPipeline : IRequestPipeline
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ICollection<IRequestPipelinePlugin> _plugins;

    public RequestPipeline(ServiceProvider serviceProvider, ICollection<IRequestPipelinePlugin> plugins)
    {
        _serviceProvider = serviceProvider;
        _plugins = plugins;
    }

    public Task<HttpResponse> ExecuteAsync(Func<RequestPipelineContext, Task<HttpResponse>> requestHandler)
    {
        var requestPipelineContext = new RequestPipelineContext();
        var pipeline = _plugins
            .Select<IRequestPipelinePlugin, Func<RequestPipelineContext, Func<RequestPipelineContext, Task<HttpResponse>>, Task<HttpResponse>>>(p => p.InvokeAsync)
            .Aggregate(
                seed: requestHandler,
                func: (next, plugin) => ctx => plugin.Invoke(ctx, next));

        return pipeline(requestPipelineContext);
    }
}