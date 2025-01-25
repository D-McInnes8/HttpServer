using System.Text;

namespace HttpServer.Response.Body;

/// <summary>
/// Represents a string body for a HTTP response.
/// </summary>
public class StringBodyContent : IHttpBodyContent
{
    /// <summary>
    /// An empty <see cref="IHttpBodyContent"/>.
    /// </summary>
    public static readonly IHttpBodyContent Empty = new StringBodyContent(string.Empty);
    
    /// <summary>
    /// The default content type for <see cref="StringBodyContent"/>.
    /// </summary>
    private static readonly HttpContentType DefaultContentType = HttpContentType.TextPlain;
    
    /// <summary>
    /// The default encoding for <see cref="StringBodyContent"/>.
    /// </summary>
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;
    
    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified content.
    /// </summary>
    /// <param name="content"></param>
    public StringBodyContent(string content) : this(content, DefaultContentType, DefaultEncoding)
    {
    }

    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified content and encoding.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="encoding"></param>
    public StringBodyContent(string content, Encoding encoding) : this(content, DefaultContentType, encoding)
    {
    }
    
    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified content and content type.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentType"></param>
    /// <param name="encoding"></param>
    public StringBodyContent(string content, HttpContentType contentType, Encoding encoding)
    {
        Content = encoding.GetBytes(content);
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