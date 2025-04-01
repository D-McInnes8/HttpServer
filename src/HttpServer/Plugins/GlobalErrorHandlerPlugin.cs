using HttpServer.Pipeline;
using HttpServer.Response;
using Microsoft.Extensions.Logging;

namespace HttpServer.Plugins;

/// <summary>
/// A global error handler plugin that catches exceptions thrown by the request pipeline.
/// </summary>
public class GlobalErrorHandlerPlugin : IRequestPipelinePlugin
{
    private readonly ILogger<GlobalErrorHandlerPlugin> _logger;

    /// <summary>
    /// Creates a new <see cref="GlobalErrorHandlerPlugin"/> with the specified logger.
    /// </summary>
    /// <param name="logger"></param>
    public GlobalErrorHandlerPlugin(ILogger<GlobalErrorHandlerPlugin> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<HttpResponse> InvokeAsync(RequestPipelineContext context, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return HttpResponse.InternalServerError();
        }
    }
}