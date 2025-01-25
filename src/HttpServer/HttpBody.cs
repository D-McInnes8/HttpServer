using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HttpServer;

/// <summary>
/// Represents the body of a HTTP request or response.
/// </summary>
public interface IHttpBodyContent
{
    /// <summary>
    /// The content type of the body.
    /// </summary>
    public HttpContentType ContentType { get; }
    
    /// <summary>
    /// The encoding of the body.
    /// </summary>
    public Encoding Encoding { get; }
    
    /// <summary>
    /// The content of the body.
    /// </summary>
    public byte[] Content { get; }
    
    /// <summary>
    /// The length of the content.
    /// </summary>
    public int Length => Content.Length;
    
    /// <summary>
    /// Copies the body to a destination span.
    /// </summary>
    /// <param name="destination">The <see cref="Span{T}"/> to be copied to.</param>
    public void CopyTo(Span<byte> destination);
}

/// <summary>
/// Represents the body of a HTTP request or response.
/// </summary>
public class HttpBody
{
    /// <summary>
    /// The content type of the body.
    /// </summary>
    public required HttpContentType ContentType { get; init; }
    
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
    public HttpBody(HttpContentType contentType, string content)
    {
        ContentType = contentType;
        Content = content;
    }
}