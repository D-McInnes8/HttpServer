using System.Text;
using HttpServer.Headers;

namespace HttpServer.Body;

/// <summary>
/// Represents a string body for a HTTP response.
/// </summary>
public class StringBodyContent : HttpBodyContent
{
    /// <summary>
    /// An empty <see cref="HttpBodyContent"/>.
    /// </summary>
    public static readonly HttpBodyContent Empty = new StringBodyContent(string.Empty);
    
    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified content.
    /// </summary>
    /// <param name="content">The content of the body.</param>
    public StringBodyContent(string content) : this(content, HttpContentType.TextPlain, Encoding.Default)
    {
    }

    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified content and encoding.
    /// </summary>
    /// <param name="content">The content of the body.</param>
    /// <param name="encoding">The encoding of the body.</param>
    public StringBodyContent(string content, Encoding encoding) : this(content, HttpContentType.TextPlain, encoding)
    {
    }
    
    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified content and content type.
    /// </summary>
    /// <param name="content">The content of the body.</param>
    /// <param name="contentType">The content type of the body.</param>
    public StringBodyContent(string content, HttpContentType contentType)
    {
        ArgumentNullException.ThrowIfNull(contentType);
        Encoding = contentType.Charset is not null ? Encoding.GetEncoding(contentType.Charset) : Encoding.Default;
        Content = Encoding.GetBytes(content);
        ContentType = contentType;
    }
    
    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified byte array and content type.
    /// </summary>
    /// <param name="content">The content of the body as a byte array.</param>
    /// <param name="contentType">The content type of the body.</param>
    /// <param name="encoding">The encoding of the body.</param>
    public StringBodyContent(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(contentType);
        ArgumentNullException.ThrowIfNull(encoding);
        Content = content;
        ContentType = contentType;
        Encoding = encoding;
    }
    
    /// <summary>
    /// Constructs a new <see cref="StringBodyContent"/> with the specified content and content type.
    /// </summary>
    /// <param name="content">The content of the body.</param>
    /// <param name="contentType">The content type of the body.</param>
    /// <param name="encoding">The encoding of the body.</param>
    public StringBodyContent(string content, HttpContentType contentType, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(contentType);
        ArgumentNullException.ThrowIfNull(encoding);
        Content = encoding.GetBytes(content);
        ContentType = contentType;
        Encoding = encoding;
    }
    
    /// <summary>
    /// Gets the content of the body as a string.
    /// </summary>
    /// <returns>The content of the body as a string.</returns>
    public string GetStringContent() => Encoding.GetString(Content);
    
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