using Application.Pipeline;

namespace Application.Routing;

/// <summary>
/// 
/// </summary>
public class DefaultRouter : IRouter
{
    /// <inheritdoc />
    public Task<RouterResult> RouteAsync(RequestPipelineContext ctx)
    {
        return Task.FromResult(RouterResult.Success);
    }
}