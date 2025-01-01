using System.Diagnostics.CodeAnalysis;

namespace HttpServer;

public class HttpBody
{
    public required string ContentType { get; init; }
    public required string Content { get; init; }
    
    public HttpBody()
    {
    }
    
    [SetsRequiredMembers]
    public HttpBody(string contentType, string content)
    {
        ContentType = contentType;
        Content = content;
    }
}