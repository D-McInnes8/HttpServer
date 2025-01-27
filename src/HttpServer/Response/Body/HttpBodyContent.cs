using System.Text;

namespace HttpServer.Response.Body;

/// <summary>
/// Represents the body of a HTTP request or response.
/// </summary>
public interface HttpBodyContent
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