using HttpServer.Pipeline;
using HttpServer.Response;

namespace HttpServer.Plugins.ResponseCompression;

/// <summary>
/// A plugin for compressing the response body.
/// </summary>
public class ResponseCompressionPlugin : IRequestPipelinePlugin
{
    public async Task<HttpResponse> InvokeAsync(RequestPipelineContext ctx, Func<RequestPipelineContext, Task<HttpResponse>> next)
    {
        var response = await next(ctx);
        if (ctx.Request.AcceptEncoding is null)
        {
            return response;
        }

        var acceptedEncodings = ctx.Request.AcceptEncoding.Encodings;
        foreach (var encoding in acceptedEncodings)
        {
            ICompressionProvider? compressionProvider = encoding switch
            {
                "gzip" => new GZipCompressionProvider(),
                "br" => new BrotliCompressionProvider(),
                "deflate" => new DeflateCompressionProvider(),
                _ => null
            };
            
            if (compressionProvider is not null && response.Body is not null)
            {
                response.Headers.Add("Content-Encoding", compressionProvider.EncodingName);
                response.Headers.Add("Vary", "Accept-Encoding");
                //ctx.ResponseWriter = new ResponseCompressionWriter(ctx.ResponseWriter, compressionProvider);
                //ctx.ResponseBodyWriter = compressionProvider.GetCompressionStream(ctx.ResponseBodyWriter);
                response.Body = new CompressedBodyContent(response.Body, compressionProvider);
                break;
            }
        }
        
        return response;
    }
}