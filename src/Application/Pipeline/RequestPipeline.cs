using Application.Response;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

/// <summary>
/// Represents a request pipeline that can be used to process incoming HTTP requests.
/// </summary>
public interface IRequestPipeline
{
    /// <summary>
    /// The name of the pipeline. Each pipeline has a unique name.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The options used to build the pipeline. This may be the base <see cref="RequestPipelineBuilderOptions"/> class,
    /// or a derived class that contains additional options.
    /// </summary>
    public RequestPipelineBuilderOptions Options { get; }
    
    /// <summary>
    /// Executes the request pipeline.
    /// </summary>
    /// <param name="ctx">The <see cref="RequestPipelineContext"/> associated with the request.</param>
    /// <returns>A <see cref="HttpResponse"/> which will be returned to the original sender of the request.</returns>
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