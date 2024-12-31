using Application.Pipeline;

namespace Application.Routing;

/// <summary>
/// A default router that will be used if no other router is found. This router will always return a successful result.
/// </summary>
public class DefaultRouter : IRouter
{
    /// <inheritdoc />
    public Task<RouterResult> RouteAsync(RequestPipelineContext ctx)
    {
        return Task.FromResult(RouterResult.Success);
    }
}