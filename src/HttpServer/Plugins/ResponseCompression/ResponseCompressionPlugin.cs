using HttpServer.Pipeline;
using HttpServer.Response;

namespace HttpServer.Plugins.ResponseCompression;

/// <summary>
/// A plugin for compressing the response body.
/// </summary>
public class ResponseCompressionPlugin : IRequestPipelinePlugin
{
    public async Task<HttpResponse> InvokeAsync(RequestPipelineContext ctx, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        var response = await next(ctx);
        return response;
    }
}