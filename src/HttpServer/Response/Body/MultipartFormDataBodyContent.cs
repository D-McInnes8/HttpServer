using System.Collections;
using System.Text;
using HttpServer.Headers;

namespace HttpServer.Response.Body;

/// <summary>
/// Represents a multipart form data body for a HTTP request or response.
/// </summary>
public class MultipartFormDataBodyContent : HttpBodyContent, IReadOnlyCollection<HttpBodyContent>
{
    public HttpContentType ContentType { get; }
    public Encoding Encoding { get; }
    public byte[] Content { get; }

    /// <summary>
    /// The number of parts in the multipart form data.
    /// </summary>
    public int Count => throw new NotImplementedException();
    
    /// <summary>
    /// Constructs a new <see cref="MultipartFormDataBodyContent"/> with the specified boundary.
    /// </summary>
    /// <param name="boundary">The boundary of the multipart form data.</param>
    public MultipartFormDataBodyContent(string boundary) : this([], boundary)
    {
    }
    
    /// <summary>
    /// Constructs a new <see cref="MultipartFormDataBodyContent"/> with the specified content and boundary.
    /// </summary>
    /// <param name="content">The content of the body.</param>
    /// <param name="boundary">The boundary of the multipart form data.</param>
    public MultipartFormDataBodyContent(byte[] content, string boundary)
        : this(content, new HttpContentType($"multipart/form-data; boundary={boundary}"), Encoding.ASCII)
    {
    }
    
    /// <summary>
    /// Constructs a new <see cref="MultipartFormDataBodyContent"/> with the specified content, content type, and encoding.
    /// </summary>
    /// <param name="content">The content of the body.</param>
    /// <param name="contentType">The content type of the body.</param>
    /// <param name="encoding">The encoding of the body.</param>
    private MultipartFormDataBodyContent(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(contentType);
        ArgumentNullException.ThrowIfNull(encoding);
        Content = content;
        ContentType = contentType;
        Encoding = encoding;
    }
    
    public HttpBodyContent this[string name] => throw new NotImplementedException();
    public HttpBodyContent this[string name, string fileName] => throw new NotImplementedException();
    public HttpBodyContent this[int index] => throw new NotImplementedException();

    public ContentDisposition? ContentDisposition => throw new NotImplementedException();

    public void CopyTo(Span<byte> destination)
    {
        Content.CopyTo(destination);
    }

    public IEnumerator<HttpBodyContent> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public bool Contains(string name) => throw new NotImplementedException();
    
    public void Add(HttpBodyContent content) => throw new NotImplementedException();
    public void Add(string name, HttpBodyContent content) => throw new NotImplementedException();
    public void Add(string name, string fileName, HttpBodyContent content) => throw new NotImplementedException();
    
    public void Remove(string name) => throw new NotImplementedException();
    public void Remove(HttpBodyContent content) => throw new NotImplementedException();
}