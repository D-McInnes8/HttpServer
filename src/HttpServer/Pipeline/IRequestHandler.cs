using HttpServer.Response;

namespace HttpServer.Pipeline;

/// <summary>
/// Interface for request handlers to be used by request pipelines. Request handlers are the final stage of the request pipeline.
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// Handles the request. This method will be invoked at the end of the request pipeline.
    /// This method should return a <see cref="HttpResponse"/> object which will be returned to the sender of the request.
    /// </summary>
    /// <param name="ctx">The <see cref="RequestPipelineContext"/> object for this request.</param>
    /// <returns>A <see cref="HttpResponse"/> which will be returned to the sender of the request.</returns>
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx);
}