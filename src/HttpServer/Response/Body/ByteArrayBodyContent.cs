using System.Text;

namespace HttpServer.Response.Body;

/// <summary>
/// Represents a byte array body for a HTTP response.
/// </summary>
public class ByteArrayBodyContent : HttpBodyContent
{
    /// <summary>
    /// Constructs a new <see cref="ByteArrayBodyContent"/> with the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentType"></param>
    /// <param name="encoding"></param>
    public ByteArrayBodyContent(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        Content = content;
        ContentType = contentType;
        Encoding = encoding;
    }
    
    /// <inheritdoc />
    public HttpContentType ContentType { get; }
    
    /// <inheritdoc />
    public Encoding Encoding { get; }
    
    /// <inheritdoc />
    public byte[] Content { get; }
    
    /// <inheritdoc />
    public void CopyTo(Span<byte> destination)
    {
        Content.CopyTo(destination);
    }
}