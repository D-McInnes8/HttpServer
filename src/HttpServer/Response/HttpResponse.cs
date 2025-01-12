using System.Diagnostics.CodeAnalysis;

namespace HttpServer.Response;

/// <summary>
/// Represents a HTTP response.
/// </summary>
public class HttpResponse
{
    /// <summary>
    /// The status code of the HTTP response.
    /// </summary>
    public required HttpResponseStatusCode StatusCode { get; set; }
    
    /// <summary>
    /// The HTTP headers to be sent with the response.
    /// </summary>
    public required Dictionary<string, string> Headers { get; set; }
    
    /// <summary>
    /// The body of the HTTP response.
    /// </summary>
    public HttpBody? Body { get; set; }
    
    /// <summary>
    /// The HTTP version of the response.
    /// </summary>
    public string HttpVersion { get; set; } = "HTTP/1.1";
    
    /// <summary>
    /// Constructs a new <see cref="HttpResponse"/> with the specified status code.
    /// </summary>
    /// <param name="statusCode">The <see cref="HttpResponseStatusCode"/> to be sent with the response.</param>
    [SetsRequiredMembers]
    public HttpResponse(HttpResponseStatusCode statusCode)
    {
        StatusCode = statusCode;
        Headers = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Constructs a new <see cref="HttpResponse"/> with the specified status code and body.
    /// </summary>
    /// <param name="statusCode">The <see cref="HttpResponseStatusCode"/> to be sent with the response.</param>
    /// <param name="body">The response body to be sent with the response.</param>
    [SetsRequiredMembers]
    public HttpResponse(HttpResponseStatusCode statusCode, HttpBody body)
    {
        StatusCode = statusCode;
        Headers = new Dictionary<string, string>();
        Body = body;
    }
    
    /// <summary>
    /// Adds a HTTP header to the response.
    /// </summary>
    /// <param name="key">The key of the HTTP header to be added.</param>
    /// <param name="value">The value of the HTTP header to be added.</param>
    public void AddHeader(string key, string value)
    {
        Headers.Add(key, value);
    }
    
    /// <summary>
    /// Sets the body of the response.
    /// </summary>
    /// <param name="body"></param>
    public void SetBody(HttpBody body)
    {
        Body = body;
    }

    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 200 OK.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse Ok() => new HttpResponse(HttpResponseStatusCode.OK);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 200 OK and the provided body.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static HttpResponse Ok(HttpBody body) => new HttpResponse(HttpResponseStatusCode.OK, body);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 200 OK and the provided string as a plain text body.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static HttpResponse Ok(string body) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody(HttpContentType.TextPlain, body));
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 404 Not Found.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse NotFound() => new HttpResponse(HttpResponseStatusCode.NotFound);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 400 Bad Request.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse BadRequest() => new HttpResponse(HttpResponseStatusCode.BadRequest);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 400 Bad Request and the provided body.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static HttpResponse BadRequest(string body) => new HttpResponse(HttpResponseStatusCode.BadRequest, new HttpBody(HttpContentType.TextPlain, body));
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 401 Unauthorized.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse Unauthorized() => new HttpResponse(HttpResponseStatusCode.Unauthorized);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 403 Forbidden.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse Forbidden() => new HttpResponse(HttpResponseStatusCode.Forbidden);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 500 Internal Server Error.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse InternalServerError() => new HttpResponse(HttpResponseStatusCode.InternalServerError);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 501 Not Implemented.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse NotImplemented() => new HttpResponse(HttpResponseStatusCode.NotImplemented);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 405 Method Not Allowed.
    /// </summary>
    /// <returns></returns>
    public static HttpResponse MethodNotAllowed() => new HttpResponse(HttpResponseStatusCode.MethodNotAllowed);
}