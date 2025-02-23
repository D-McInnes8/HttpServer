using HttpServer.Headers;
using HttpServer.Response;

namespace HttpServer.Pipeline.Plugins;

/// <summary>
/// A plugin that adds keep-alive settings to the response.
/// </summary>
public class KeepAlivePlugin : IRequestPipelinePlugin
{
    private readonly TimeSpan _timeout;
    private readonly int? _maxRequests;

    /// <summary>
    /// Constructs a new <see cref="KeepAlivePlugin"/> with the specified <see cref="HttpWebServerOptions"/>.
    /// </summary>
    /// <param name="webServerOptions">The options for the web server.</param>
    public KeepAlivePlugin(HttpWebServerOptions webServerOptions)
    {
        _timeout = webServerOptions.KeepAlive.Timeout;
        _maxRequests = webServerOptions.KeepAlive.MaxRequests;
    }

    /// <inheritdoc />
    public async Task<HttpResponse> InvokeAsync(RequestPipelineContext ctx, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        var response = await next(ctx);
        if (ctx.Request.Headers["Connection"] is null)
        {
            return response;
        }
        
        response.KeepAlive.Connection = ctx.Request.Headers["Connection"] == "keep-alive" ? HttpConnectionType.KeepAlive : HttpConnectionType.Close;
        response.KeepAlive.Timeout = _timeout;
        response.KeepAlive.MaxRequests = _maxRequests;
        response.Headers.Add("Connection", response.KeepAlive.Connection == HttpConnectionType.KeepAlive ? "keep-alive" : "close");
        response.Headers.Add("Keep-Alive", $"timeout={_timeout.TotalSeconds}, max={_maxRequests}");
        return response;
    }
}