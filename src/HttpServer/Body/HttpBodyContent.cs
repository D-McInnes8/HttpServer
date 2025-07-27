using System.Text;
using HttpServer.Headers;

namespace HttpServer.Body;

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
    public int Length { get; }
    
    /// <summary>
    /// The content disposition of the body.
    /// </summary>
    public ContentDisposition? ContentDisposition { get; set; }
    
    /// <summary>
    /// Copies the body to a destination span.
    /// </summary>
    /// <param name="destination">The <see cref="Span{T}"/> to be copied to.</param>
    public void CopyTo(Span<byte> destination);
    
    /// <summary>
    /// Copies the body content to a specified stream.
    /// </summary>
    /// <param name="destination">Destination stream to copy the content to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the destination stream is null.</exception>
    public void CopyTo(Stream destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var contentSpan = AsReadOnlySpan();
        if (contentSpan.Length > 0)
        {
            destination.Write(contentSpan);
        }
    }
    
    public byte[] ToArray()
    {
        var contentSpan = AsReadOnlySpan();
        var result = new byte[contentSpan.Length];
        contentSpan.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Returns the content as a read-only span of bytes.
    /// </summary>
    /// <returns>A read-only span of bytes representing the content.</returns>
    public ReadOnlySpan<byte> AsReadOnlySpan();
}