using HttpServer.Headers;

namespace HttpServer.Routing.StaticFiles;

/// <summary>
/// The default implementation of <see cref="IFileContentTypeProvider"/>. The static file request handler
/// will fall back to this implementation if no other content type provider is specified.
/// </summary>
public class DefaultContentTypeProvider : IFileContentTypeProvider
{
    /// <inheritdoc />
    public HttpContentType GetContentType(string extension)
    {
        return extension switch
        {
            ".html" => HttpContentType.TextHtml,
            ".htm" => HttpContentType.TextHtml,
            ".xml" => HttpContentType.TextXml,
            ".json" => HttpContentType.TextJson,
            ".csv" => HttpContentType.TextCsv,
            ".js" => HttpContentType.ApplicationJson,
            _ => HttpContentType.TextPlain
        };
    }
}