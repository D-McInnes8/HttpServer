using Application.Pipeline;
using Application.Response;

namespace Tests.IntegrationTests.TestPipelines;

public class TestRequestHandler : IRequestHandler
{
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        return Task.FromResult(HttpResponse.Ok("Hello, World!"));
    }
}