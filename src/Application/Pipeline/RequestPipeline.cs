using Application.Response;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

/// <summary>
/// 
/// </summary>
public interface IRequestPipeline
{
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public RequestPipelineBuilderOptions Options { get; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    Task<HttpResponse> ExecuteAsync(RequestPipelineContext ctx);
}

/// <inheritdoc cref="IRequestPipeline" />
public class RequestPipeline : IRequestPipeline
{
    public string Name => Options.Name;
    public RequestPipelineBuilderOptions Options { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public RequestPipeline(RequestPipelineBuilderOptions options)
    {
        Options = options;
    }

    public async Task<HttpResponse> ExecuteAsync(RequestPipelineContext requestPipelineContext)
    {
        var pipeline = Options.Plugins
            .Aggregate(
                seed: ExecuteRequestHandler,
                func: (next, type) => ctx
                    => ((IRequestPipelinePlugin)ctx.Services.GetRequiredService(type)).InvokeAsync(ctx, next));
        
        return await pipeline(requestPipelineContext);
    }
    
    private Task<HttpResponse> ExecuteRequestHandler(RequestPipelineContext ctx)
    {
        var requestHandler = ActivatorUtilities.GetServiceOrCreateInstance(ctx.Services, Options.RequestHandler);
        if (requestHandler is IRequestHandler handler)
        {
            return handler.HandleAsync(ctx);
        }

        return Task.FromResult(HttpResponse.InternalServerError());
    }
}