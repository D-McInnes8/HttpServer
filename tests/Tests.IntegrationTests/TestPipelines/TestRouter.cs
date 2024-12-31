using Application.Pipeline;
using Application.Routing;

namespace Tests.IntegrationTests.TestPipelines;

public class TestRouter : IRouter
{
    public Task<RouterResult> RouteAsync(RequestPipelineContext ctx)
    {
        if (ctx.Request.Route == "/test")
        {
            return Task.FromResult(RouterResult.Success);
        }
        
        return Task.FromResult(RouterResult.NotFound);
    }
}