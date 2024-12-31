using Application.Response;

namespace Application.Pipeline;

/// <summary>
/// Interface for request pipeline plugins. Request pipeline plugins are used to add additional functionality to the request pipeline.
/// </summary>
public interface IRequestPipelinePlugin
{
    /// <summary>
    /// Invokes the request pipeline plugin. This method should call the <paramref name="next"/> function to continue the request pipeline.
    /// Otherwise, the request pipeline will be terminated and the returned <see cref="HttpResponse"/> will be returned to the caller.
    /// </summary>
    /// <param name="context">The <see cref="RequestPipelineContext"/> associated with this request.</param>
    /// <param name="next">The delegate to execute the next plugin in the request pipeline.</param>
    /// <returns>The <see cref="HttpResponse"/> object which will be returned to the caller.</returns>
    Task<HttpResponse> InvokeAsync(RequestPipelineContext context, Func<RequestPipelineContext, Task<HttpResponse>> next);
}