using HttpServer.Response;
using Microsoft.Extensions.Logging;

namespace HttpServer.Pipeline.Plugins;

public class GlobalErrorHandlerPlugin : IRequestPipelinePlugin
{
    private readonly ILogger<GlobalErrorHandlerPlugin> _logger;

    public GlobalErrorHandlerPlugin(ILogger<GlobalErrorHandlerPlugin> logger)
    {
        _logger = logger;
    }

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