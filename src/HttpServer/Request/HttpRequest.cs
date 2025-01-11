using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using HttpServer.Request.Parser;

namespace HttpServer.Request;

/// <summary>
/// Represents a HTTP request.
/// </summary>
public class HttpRequest
{
    /// <summary>
    /// The method of the HTTP request.
    /// </summary>
    public required HttpRequestMethod Method { get; init; }
    
    /// <summary>
    /// The HTTP version of the request.
    /// </summary>
    public string HttpVersion { get; init; } = "HTTP/1.1";
    
    /// <summary>
    /// The full path of the request.
    /// </summary>
    public required string Path { get; init; }
    
    /// <summary>
    /// The route of the request.
    /// </summary>
    public required string Route { get; init; }
    
    /// <summary>
    /// Indicates if the request has a body.
    /// </summary>
    public bool HasBody => Body is not null;
    
    /// <summary>
    /// The body of the request, if one is present. Otherwise null.
    /// </summary>
    public string? Body { get; init; }
    
    /// <summary>
    /// The HTTP headers sent with the request.
    /// </summary>
    public required Dictionary<string, string> Headers { get; init; }
    
    /// <summary>
    /// The content type of the request body. Populated from the Content-Type header.
    /// </summary>
    public HttpContentType? ContentType { get; init; }
    
    /// <summary>
    /// The query parameters of the request.
    /// </summary>
    public NameValueCollection QueryParameters { get; init; }

    /// <summary>
    /// Constructs a new <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMethod"/> of the request.</param>
    /// <param name="path">The full path of the HTTP request.</param>
    [SetsRequiredMembers]
    public HttpRequest(HttpRequestMethod method, string path)
    {
        Method = method;
        Path = path;
        Headers = new Dictionary<string, string>();
        QueryParameters = new NameValueCollection();

        var (route, parameters) = HttpRequestParser.ParsePath(Path);
        Route = route;
        QueryParameters = parameters;
    }
}