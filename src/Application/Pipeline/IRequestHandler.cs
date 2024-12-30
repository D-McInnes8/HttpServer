using Application.Response;

namespace Application.Pipeline;

/// <summary>
/// 
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx);
}