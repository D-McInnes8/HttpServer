using Application.Pipeline;
using Application.Response;

namespace Tests.IntegrationTests.TestPipelines;

public class TestPlugin : IRequestPipelinePlugin
{
    public Task<HttpResponse> InvokeAsync(RequestPipelineContext context, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        return Task.FromResult(HttpResponse.NotImplemented());
    }
}