using HttpServer.Pipeline;
using HttpServer.Response;

namespace Server;

public class TestPlugin : IRequestPipelinePlugin
{
    public async Task<HttpResponse> InvokeAsync(RequestPipelineContext context, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        var response = await next(context);
        response.AddHeader("X-Test-Plugin", "true");
        return response;
    }
}