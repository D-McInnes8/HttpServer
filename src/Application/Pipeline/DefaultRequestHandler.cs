using Application.Response;

namespace Application.Pipeline;

/// <summary>
/// 
/// </summary>
public class DefaultRequestHandler : IRequestHandler
{
    /// <inheritdoc />
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        return Task.FromResult(HttpResponse.NotFound());
    }
}