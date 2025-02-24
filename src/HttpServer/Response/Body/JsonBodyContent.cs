using System.Text;
using System.Text.Json;
using HttpServer.Headers;

namespace HttpServer.Response.Body;

/// <summary>
/// Represents a JSON body for a HTTP response.
/// </summary>
public class JsonBodyContent<T> : HttpBodyContent
{
    /// <summary>
    /// Constructs a new <see cref="JsonBodyContent{T}"/> with the specified content.
    /// </summary>
    /// <param name="content"></param>
    public JsonBodyContent(T content) : this(content, HttpContentType.ApplicationJson, Encoding.UTF8)
    {
    }
    
    /// <summary>
    /// Constructs a new <see cref="JsonBodyContent{T}"/> with the specified content and encoding.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentType"></param>
    /// <param name="encoding"></param>
    public JsonBodyContent(T content, HttpContentType contentType, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        Content = Equals(encoding, Encoding.UTF8)
            ? JsonSerializer.SerializeToUtf8Bytes(content)
            : encoding.GetBytes(JsonSerializer.Serialize(content));
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
    public ContentDisposition? ContentDisposition => throw new NotImplementedException();

    /// <inheritdoc />
    public void CopyTo(Span<byte> destination)
    {
        Content.CopyTo(destination);
    }
}