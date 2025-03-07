using System.Collections;
using System.Text;
using HttpServer.Headers;
using HttpServer.Request.Parser;

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
    public MultipartFormDataBodyContent(byte[] content, HttpContentType contentType, Encoding encoding)
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
    
    public static MultipartFormDataBodyContent Parse(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType.Boundary, nameof(contentType.Boundary));
        var original = encoding.GetString(content);
        ReadOnlySpan<byte> span = content.AsSpan();
        byte[] boundary = encoding.GetBytes(contentType.Boundary);
        
        var reader = new MultipartContentReader(content, encoding, contentType.Boundary);
        while (!reader.IsFinalBoundary())
        {
            var partContent = reader.ReadToNextBoundary();
        }
        
        return new MultipartFormDataBodyContent(content, contentType, encoding);
    }
}

/// <summary>
/// Represents a reader for a multipart form data content.
/// </summary>
public ref struct MultipartContentReader
{
    private readonly ReadOnlySpan<byte> _content;
    private readonly Encoding _encoding;
    private readonly ReadOnlySpan<byte> _boundary;
    
    private int _position;
    private readonly int _finalBoundaryIndex;
    
    /// <summary>
    /// Constructs a new <see cref="MultipartContentReader"/> with the specified content, encoding, and boundary.
    /// </summary>
    /// <param name="content">The content to read.</param>
    /// <param name="encoding">The encoding of the content.</param>
    /// <param name="boundary">The boundary of the parts.</param>
    /// <exception cref="HttpParserException">Thrown when the boundary is invalid or the final boundary is missing.</exception>
    public MultipartContentReader(ReadOnlySpan<byte> content, Encoding encoding, string boundary)
    {
        _content = content;
        _encoding = encoding;
        _boundary = _encoding.GetBytes($"--{boundary}");
        
        var firstBoundaryIndex = _content.IndexOf(_boundary);
        if (firstBoundaryIndex == -1)
        {
            throw new MultipartParserException("Invalid boundary");
        }
        
        _position = firstBoundaryIndex + _boundary.Length;
        if (content[_position] == '\r' && content[_position + 1] == '\n')
        {
            _position += 2;
        }
        else if (content[_position] == '\n')
        {
            _position += 1;
        }
        
        _finalBoundaryIndex = _content.IndexOf(_encoding.GetBytes($"\n--{boundary}--"));
        if (_finalBoundaryIndex == -1)
        {
            throw new MultipartParserException("No final boundary found");
        }
    }
    
    /// <summary>
    /// Reads the next line of the content.
    /// </summary>
    /// <returns>The read line.</returns>
    public string ReadLine()
    {
        var newLineIndex = _content[_position..].IndexOfAny((byte)'\r', (byte)'\n');
        if (newLineIndex == -1)
        {
            return _encoding.GetString(_content[_position..]);
        }
        
        var result = _content.Slice(_position, newLineIndex);
        _position += newLineIndex + 1;
        
        if (_position < _content.Length
            && _content[_position - 1] == '\r'
            && _content[_position] == '\n')
        {
            _position++;
        }
        
        return _encoding.GetString(result);
    }

    /// <summary>
    /// Reads to the next boundary.
    /// </summary>
    /// <returns>The content up to the next boundary.</returns>
    /// <exception cref="HttpParserException">Thrown when the final boundary is reached or the boundary is not found.</exception>
    public ReadOnlySpan<byte> ReadToNextBoundary()
    {
        if (_position >= _finalBoundaryIndex)
        {
            throw new MultipartParserException("Have reached the final boundary");
        }
        
        var boundaryIndex = _content[_position..].IndexOf(_boundary);
        if (boundaryIndex == -1)
        {
            throw new MultipartParserException("No boundary found");
        }

        var result = _content.Slice(_position, boundaryIndex);
        _position += boundaryIndex + _boundary.Length;
        
        // Strip the newline characters from the start of the result.
        if (result[0] == '\r' && result[1] == '\n')
        {
            result = result[2..];
        }
        else if (result[0] == '\n')
        {
            result = result[1..];
        }
        
        // Strip the newline characters from the end of the result.
        if (result[^2] == '\r' && result[^1] == '\n')
        {
            result = result[..^2];
        }
        else if (result[^1] == '\n')
        {
            result = result[..^1];
        }
        
        return result;
    }
    
    /// <summary>
    /// Checks if the current position is at the final boundary.
    /// </summary>
    /// <returns><see langword="true"/> if the current position is at the final boundary; otherwise, <see langword="false"/>.</returns>
    public bool IsFinalBoundary()
    {
        return _position >= _finalBoundaryIndex;
    }
}