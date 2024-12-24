using Application.Response;

namespace Application.Pipeline;

public interface IRequestPipelinePlugin
{
    Task<HttpResponse> InvokeAsync(RequestPipelineContext context, Func<RequestPipelineContext, Task<HttpResponse>> next);
}