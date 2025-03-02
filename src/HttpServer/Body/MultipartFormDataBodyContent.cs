using System.Collections;
using System.Text;
using HttpServer.Headers;

namespace HttpServer.Body;

/// <summary>
/// Represents a multipart form data body for a HTTP request or response.
/// </summary>
public class MultipartFormDataBodyContent : HttpBodyContent, IReadOnlyCollection<HttpBodyContent>
{
    private readonly List<HttpBodyContent> _parts = new List<HttpBodyContent>();
    
    public HttpContentType ContentType { get; }
    public Encoding Encoding { get; }
    public byte[] Content { get; }
    
    /// <summary>
    /// The boundary of the multipart form data.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the boundary is missing.</exception>
    public string Boundary => ContentType.Parameters["boundary"] ?? throw new InvalidOperationException("Missing boundary");

    /// <summary>
    /// The number of parts in the multipart form data.
    /// </summary>
    public int Count => _parts.Count;
    
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
        : this(content, new HttpContentType($"multipart/form-data; boundary={boundary}"), Encoding.UTF8)
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
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType.Boundary, nameof(contentType.Boundary));
        
        Content = content;
        ContentType = contentType;
        Encoding = encoding;
    }

    /// <summary>
    /// Gets the part with the specified name.
    /// </summary>
    /// <param name="name">The name of the part.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the part with the specified name is not found.</exception>
    public HttpBodyContent this[string name]
    {
        get
        {
            var result = _parts.SingleOrDefault(x => x.ContentDisposition?.Name == name);
            if (result is null)
            {
                throw new KeyNotFoundException($"Part with name '{name}' not found");
            }
            
            return result;
        }
    }
    
    public HttpBodyContent this[string name, string fileName] => throw new NotImplementedException();
    public HttpBodyContent this[int index] => throw new NotImplementedException();

    public ContentDisposition? ContentDisposition { get; set; }

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
    
    /// <summary>
    /// Checks if the multipart form data contains a part with the specified name.
    /// </summary>
    /// <param name="name">The name of the part.</param>
    /// <returns><see langword="true"/> if the part exists; otherwise, <see langword="false"/>.</returns>
    public bool Contains(string name) => _parts.Any(x => x.ContentDisposition?.Name == name);
    
    /// <summary>
    /// Adds a part to the multipart form data.
    /// </summary>
    /// <param name="content">The part to add.</param>
    public void Add(HttpBodyContent content) => throw new NotImplementedException();

    /// <summary>
    /// Adds a part to the multipart form data with the specified name.
    /// </summary>
    /// <param name="name">The name of the part.</param>
    /// <param name="content">The part to add.</param>
    public void Add(string name, HttpBodyContent content)
    {
        content.ContentDisposition ??= new ContentDisposition(name);
        _parts.Add(content);
    }

    /// <summary>
    /// Adds a part to the multipart form data with the specified name and file name.
    /// </summary>
    /// <param name="name">The name of the part.</param>
    /// <param name="fileName">The file name of the part.</param>
    /// <param name="content">The part to add.</param>
    public void Add(string name, string fileName, HttpBodyContent content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        
        content.ContentDisposition ??= new ContentDisposition(fileName, name);
        _parts.Add(content);
    }

    /// <summary>
    /// Removes a part from the multipart form data with the specified name.
    /// </summary>
    /// <param name="name">The name of the part to remove.</param>
    public void Remove(string name)
    {
        _parts.RemoveAll(x => x.ContentDisposition?.Name == name);
    }

    /// <summary>
    /// Removes a part from the multipart form data.
    /// </summary>
    /// <param name="content">The part to remove.</param>
    public void Remove(HttpBodyContent content)
    {
        _parts.Remove(content);
    }
}