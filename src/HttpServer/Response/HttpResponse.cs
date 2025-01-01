using System.Diagnostics.CodeAnalysis;

namespace HttpServer.Response;

public class HttpResponse
{
    public required HttpResponseStatusCode StatusCode { get; set; }
    
    public required Dictionary<string, string> Headers { get; set; }
    
    public HttpBody? Body { get; set; }
    
    public string HttpVersion { get; set; } = "HTTP/1.1";
    
    [SetsRequiredMembers]
    public HttpResponse(HttpResponseStatusCode statusCode)
    {
        StatusCode = statusCode;
        Headers = new Dictionary<string, string>();
    }
    
    [SetsRequiredMembers]
    public HttpResponse(HttpResponseStatusCode statusCode, HttpBody body)
    {
        StatusCode = statusCode;
        Headers = new Dictionary<string, string>();
        Body = body;
    }
    
    public void AddHeader(string key, string value)
    {
        Headers.Add(key, value);
    }
    
    public void SetBody(HttpBody body)
    {
        Body = body;
    }

    public static HttpResponse Ok() => new HttpResponse(HttpResponseStatusCode.OK);
    public static HttpResponse Ok(HttpBody body) => new HttpResponse(HttpResponseStatusCode.OK, body);
    public static HttpResponse Ok(string body) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody("text/plain", body));
    
    public static HttpResponse NotFound() => new HttpResponse(HttpResponseStatusCode.NotFound);
    public static HttpResponse BadRequest() => new HttpResponse(HttpResponseStatusCode.BadRequest);
    public static HttpResponse Unauthorized() => new HttpResponse(HttpResponseStatusCode.Unauthorized);
    public static HttpResponse Forbidden() => new HttpResponse(HttpResponseStatusCode.Forbidden);
    public static HttpResponse InternalServerError() => new HttpResponse(HttpResponseStatusCode.InternalServerError);
    public static HttpResponse NotImplemented() => new HttpResponse(HttpResponseStatusCode.NotImplemented);
}