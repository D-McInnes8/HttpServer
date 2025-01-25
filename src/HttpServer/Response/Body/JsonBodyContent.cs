using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace HttpServer.Response.Body;

/// <summary>
/// Represents a JSON body for a HTTP response.
/// </summary>
public class JsonBodyContent<T> : IHttpBodyContent
{
    /// <summary>
    /// The default content type for <see cref="JsonBodyContent{T}"/>.
    /// </summary>
    private static readonly HttpContentType DefaultContentType = HttpContentType.ApplicationJson;
    
    /// <summary>
    /// The default encoding for <see cref="JsonBodyContent{T}"/>.
    /// </summary>
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;
    
    /// <summary>
    /// Constructs a new <see cref="JsonBodyContent{T}"/> with the specified content.
    /// </summary>
    /// <param name="content"></param>
    public JsonBodyContent(T content) : this(content, DefaultContentType, DefaultEncoding)
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
        Content = JsonSerializer.SerializeToUtf8Bytes(content);
        ContentType = contentType;
        Encoding = encoding;
    }
    
    /// <summary>
    /// Constructs a new <see cref="JsonBodyContent{T}"/> from an object.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static JsonBodyContent<T> FromObject(T content) => new(content);
    
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