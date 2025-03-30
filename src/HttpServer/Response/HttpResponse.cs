using System.Diagnostics.CodeAnalysis;
using HttpServer.Body;
using HttpServer.Headers;

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
    public HttpBodyContent? Body { get; set; }
    
    /// <summary>
    /// The HTTP version of the response.
    /// </summary>
    public string HttpVersion { get; set; } = "HTTP/1.1";
    
    /// <summary>
    /// The keep-alive settings for the response.
    /// </summary>
    public HttpKeepAlive KeepAlive { get; set; }
    
    /// <summary>
    /// Constructs a new <see cref="HttpResponse"/> with the specified status code.
    /// </summary>
    /// <param name="statusCode">The <see cref="HttpResponseStatusCode"/> to be sent with the response.</param>
    [SetsRequiredMembers]
    public HttpResponse(HttpResponseStatusCode statusCode)
    {
        StatusCode = statusCode;
        Headers = new Dictionary<string, string>();
        KeepAlive = new HttpKeepAlive
        {
            Connection = HttpConnectionType.Close,
            Timeout = TimeSpan.Zero,
        };
    }
    
    /// <summary>
    /// Constructs a new <see cref="HttpResponse"/> with the specified status code and body.
    /// </summary>
    /// <param name="statusCode">The <see cref="HttpResponseStatusCode"/> to be sent with the response.</param>
    /// <param name="body">The response body to be sent with the response.</param>
    [SetsRequiredMembers]
    public HttpResponse(HttpResponseStatusCode statusCode, HttpBodyContent body)
    {
        StatusCode = statusCode;
        Headers = new Dictionary<string, string>();
        Body = body;
        KeepAlive = new HttpKeepAlive
        {
            Connection = HttpConnectionType.Close,
            Timeout = TimeSpan.Zero,
        };
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
    public void SetBody(HttpBodyContent body)
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
    public static HttpResponse Ok(HttpBodyContent body) => new HttpResponse(HttpResponseStatusCode.OK, body);
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with a status code of 200 OK and the provided string as a plain text body.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static HttpResponse Ok(string body) => new HttpResponse(HttpResponseStatusCode.OK, new StringBodyContent(body));
    
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
    public static HttpResponse BadRequest(string body) => new HttpResponse(HttpResponseStatusCode.BadRequest, new StringBodyContent(body));
    
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
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> with the specified status code and the provided object serialized as JSON.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static HttpResponse Json<T>(HttpResponseStatusCode statusCode, T value) => new HttpResponse(statusCode, new JsonBodyContent<T>(value));
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> indicating that the resource has been moved temporarily.
    /// </summary>
    /// <param name="location">The url to which the resource has been moved.</param>
    /// <returns>The <see cref="HttpResponse"/> indicating that the resource has been moved temporarily.</returns>
    public static HttpResponse Redirect(string location) => new HttpResponse(HttpResponseStatusCode.Found)
    {
        Headers =
        {
            { "Location", location },
        },
    };
    
    /// <summary>
    /// Creates a new <see cref="HttpResponse"/> indicating that the resource has been moved permanently.
    /// </summary>
    /// <param name="location">The url to which the resource has been moved.</param>
    /// <returns>The <see cref="HttpResponse"/> indicating that the resource has been moved permanently.</returns>
    public static HttpResponse MovePermanently(string location) => new HttpResponse(HttpResponseStatusCode.MovePermanently)
    {
        Headers =
        {
            { "Location", location },
        },
    };
}