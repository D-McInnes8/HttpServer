using System.Diagnostics.CodeAnalysis;

namespace HttpServer;

/// <summary>
/// Represents the body of a HTTP request or response.
/// </summary>
public class HttpBody
{
    /// <summary>
    /// The content type of the body.
    /// </summary>
    public required string ContentType { get; init; }
    
    /// <summary>
    /// The content of the body.
    /// </summary>
    public required string Content { get; init; }
    
    /// <summary>
    /// Constructs a new <see cref="HttpBody"/>.
    /// </summary>
    public HttpBody()
    {
    }
    
    /// <summary>
    /// Constructs a new <see cref="HttpBody"/> with the specified content type and content.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="content"></param>
    [SetsRequiredMembers]
    public HttpBody(string contentType, string content)
    {
        ContentType = contentType;
        Content = content;
    }
}