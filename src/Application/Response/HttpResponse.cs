using System.Diagnostics.CodeAnalysis;

namespace Application.Response;

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
}