using System.Text;
using HttpServer.Body.Serializers;
using HttpServer.Headers;

namespace HttpServer.Body;

/// <summary>
/// Represents a byte array body for a HTTP response.
/// </summary>
public class ByteArrayBodyContent : HttpBodyContent, IHttpBodyContentSerializer
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
    
    /// <summary>
    /// Constructs a new <see cref="ByteArrayBodyContent"/> with the specified content.
    /// </summary>
    /// <param name="content">The content of the body.</param>
    public ByteArrayBodyContent(byte[] content) : this(content, HttpContentType.ApplicationOctetStream, Encoding.Default)
    {
    }
    
    /// <inheritdoc />
    public HttpContentType ContentType { get; }
    
    /// <inheritdoc />
    public Encoding Encoding { get; }
    
    /// <inheritdoc />
    public byte[] Content { get; }
    
    /// <inheritdoc />
    public int Length => Content.Length;

    /// <inheritdoc />
    public ContentDisposition? ContentDisposition { get; set; }

    /// <inheritdoc />
    public void CopyTo(Span<byte> destination)
    {
        Content.CopyTo(destination);
    }
    
    public ReadOnlySpan<byte> AsReadOnlySpan()
    {
        return Content.AsSpan();
    }

    public static HttpBodyContent Deserialize(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        return new ByteArrayBodyContent(content, contentType, encoding);
    }
}