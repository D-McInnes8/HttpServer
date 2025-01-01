using HttpServer.Response;

namespace HttpServer.Pipeline.Plugins;

public class GlobalErrorHandlerPlugin : IRequestPipelinePlugin
{
    public async Task<HttpResponse> InvokeAsync(RequestPipelineContext context, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return HttpResponse.InternalServerError();
        }
    }
}