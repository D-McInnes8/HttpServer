using HttpServer.Response;

namespace HttpServer.Pipeline;

/// <summary>
/// A default request handler that will be used if no other request handler is found. This handler will return a 404 Not Found response.
/// </summary>
public class DefaultRequestHandler : IRequestHandler
{
    /// <inheritdoc />
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        return Task.FromResult(HttpResponse.NotFound());
    }
}