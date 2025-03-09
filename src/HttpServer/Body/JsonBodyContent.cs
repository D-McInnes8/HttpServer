using System.Text;
using System.Text.Json;
using HttpServer.Headers;

namespace HttpServer.Body;

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

    /// <summary>
    /// Deserializes the content to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The deserialized object.</returns>
    public T? Deserialize()
    {
        if (Equals(Encoding, Encoding.UTF8))
        {
            return JsonSerializer.Deserialize<T>(Content);
        }
        
        return JsonSerializer.Deserialize<T>(Encoding.GetString(Content));
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
}