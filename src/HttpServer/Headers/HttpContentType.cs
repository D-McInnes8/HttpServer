using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace HttpServer.Headers;

/// <summary>
/// Represents the content type of an HTTP request or response.
/// </summary>
public class HttpContentType : IHttpHeader, IEquatable<HttpContentType>, ISpanParsable<HttpContentType>
{
    /// <summary>
    /// The type of the HTTP content.
    /// </summary>
    public required string Type { get; init; } = "text";
    
    /// <summary>
    /// The subtype of the HTTP content.
    /// </summary>
    public required string SubType { get; init; } = "plain";
    
    /// <summary>
    /// The media type of the HTTP content.
    /// </summary>
    public string MediaType => $"{Type}/{SubType}";
    
    /// <summary>
    /// The parameters of the HTTP content.
    /// </summary>
    public required NameValueCollection Parameters { get; init; }

    /// <summary>
    /// The charset of the HTTP content, if the charset is not specified, returns null.
    /// </summary>
    public string? Charset
    {
        get => Parameters["charset"];
        set
        {
            if (value == null)
            {
                Parameters.Remove("charset");
            }
            else
            {
                Parameters["charset"] = value;
            }
        }
    }

    /// <summary>
    /// The boundary of the HTTP content, if the boundary is not specified, returns null.
    /// </summary>
    public string? Boundary
    {
        get => Parameters["boundary"];
        set => Parameters["boundary"] = value;
    }

    /// <summary>
    /// The rendered value of the content type.
    /// </summary>
    public string Value
    {
        get
        {
            if (Parameters.Count == 0)
            {
                return $"{Type}/{SubType}";
            }

            var collection = Parameters;
            var parameters = Parameters.AllKeys.Select(key => $"{key}={collection[key]}");
            return $"{Type}/{SubType}; {string.Join(';', parameters)}";
        }
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="HttpContentType"/>.
    /// </summary>
    /// <param name="type">The type of the HTTP content type.</param>
    /// <param name="subType">The subtype of the HTTP content type.</param>
    /// <param name="parameters">The parameters of the HTTP content type.</param>
    [SetsRequiredMembers]
    public HttpContentType(string type, string subType, NameValueCollection parameters)
    {
        Type = type;
        SubType = subType;
        Parameters = parameters;
    }

    /// <summary>
    /// Creates a new instance of <see cref="HttpContentType"/>.
    /// </summary>
    /// <param name="type">The type of the HTTP content type.</param>
    /// <param name="subType">The subtype of the HTTP content type.</param>
    /// <param name="parameters">The parameters of the HTTP content type.</param>
    [SetsRequiredMembers]
    public HttpContentType(string type, string subType, params IEnumerable<(string, string)> parameters)
    {
        Type = type;
        SubType = subType;
        Parameters = new NameValueCollection();
        foreach (var (key, value) in parameters)
        {
            Parameters.Add(key, value);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="HttpContentType"/> from the provided content type.
    /// </summary>
    /// <param name="contentType">The content type to parse.</param>
    /// <exception cref="ArgumentException">Thrown when the content type is invalid.</exception>
    [SetsRequiredMembers]
    public HttpContentType(string contentType)
    {
        if (!TryParse(contentType, out var result))
        {
            throw new ArgumentException("The format of the provided content type is invalid.", nameof(contentType));
        }
        
        Type = result.Type;
        SubType = result.SubType;
        Parameters = result.Parameters;
    }

    /// <summary>
    /// Renders the content type into a string.
    /// </summary>
    /// <returns>The content type formatted for HTTP request/responses.</returns>
    public string Render()
    {
        if (Parameters.Count == 0)
        {
            return $"{Type}/{SubType}";
        }

        var collection = Parameters;
        var parameters = Parameters.AllKeys.Select(key => $"{key}={collection[key]}");
        return $"{Type}/{SubType}; {string.Join(';', parameters)}";
    }

    /// <inheritdoc />
    public static HttpContentType Parse(string s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new ArgumentException("HttpContent value cannot be null or whitespace.", nameof(s));
        }

        return Parse(s);
    }
    
    /// <inheritdoc />
    public static HttpContentType Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(s);
    }

    /// <summary>
    /// Parses the provided string into an instance of <see cref="HttpContentType"/>.
    /// </summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <returns>The result of parsing s.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException"></exception>
    public static HttpContentType Parse(ReadOnlySpan<char> s)
    {
        if (s.Length == 0)
        {
            throw new ArgumentException("HttpContent must not be empty.", nameof(s));
        }

        if (!s.Contains('/'))
        {
            throw new ArgumentException("HttpContent must contain a sub type.", nameof(s));
        }

        if (TryParse(s, out var result))
        {
            return result;
        }
        
        throw new FormatException("The format of the provided content type is invalid.");
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out HttpContentType result)
    {
        return TryParse(s, out result);
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out HttpContentType result)
    {
        return TryParse(s, out result);
    }
    
    /// <summary>
    /// Tries to parse the provided string into an instance of <see cref="HttpContentType"/>.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryParse(ReadOnlySpan<char> s, out HttpContentType result)
    {
        if (s.Length == 0)
        {
            result = default;
            return false;
        }
        
        var typeDelimiter = s.IndexOf('/');
        if (typeDelimiter == -1)
        {
            result = default;
            return false;
        }
        
        var type = s.Slice(0, typeDelimiter).ToString();
        var subtypeAndParameters = s[(typeDelimiter + 1)..];
        
        var subTypeDelimiter = subtypeAndParameters.IndexOf(';');
        if (subTypeDelimiter == -1)
        {
            result = new HttpContentType(type, subtypeAndParameters.ToString(), new NameValueCollection());
            return true;
        }
        
        var subType = subtypeAndParameters.Slice(0, subTypeDelimiter).ToString();
        var parameters = subtypeAndParameters[(subTypeDelimiter + 1)..];
        var parametersCollection = new NameValueCollection();
        var parameterPairs = parameters.Split(';');
        foreach (var pair in parameterPairs)
        {
            var parameterSlice = parameters[pair.Start..pair.End];
            var parameterDelimiter = parameterSlice.IndexOf('=');

            if (parameterDelimiter != -1)
            {
                var parameterKey = parameterSlice.Slice(0, parameterDelimiter).Trim();
                var parameterValue = parameterSlice[(parameterDelimiter + 1)..].Trim();
                if (parameterValue[0] == '"' && parameterValue[^1] == '"')
                {
                    parameterValue = parameterValue[1..^1];
                }
                
                parametersCollection.Add(parameterKey.ToString(), parameterValue.ToString());
            }
        }
        
        result = new HttpContentType(type, subType, parametersCollection);
        return true;
    }

    /// <inheritdoc />
    public bool Equals(HttpContentType other)
    {
        return Type == other.Type
               && SubType == other.SubType;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is HttpContentType type && Equals(type);
    
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Type, SubType);
    
    /// <inheritdoc />
    public override string ToString() => $"{Type}/{SubType}";
    
    /// <summary>
    /// Creates a text/plain content type.
    /// </summary>
    public static HttpContentType TextPlain => new("text", "plain", new NameValueCollection());
    
    /// <summary>
    /// Creates a text/html content type.
    /// </summary>
    public static HttpContentType TextHtml => new("text", "html", new NameValueCollection());
    
    /// <summary>
    /// Creates a text/xml content type.
    /// </summary>
    public static HttpContentType TextXml => new("text", "xml", new NameValueCollection());
    
    /// <summary>
    /// Creates a text/json content type.
    /// </summary>
    public static HttpContentType TextJson => new("text", "json", new NameValueCollection());
    
    /// <summary>
    /// Creates a text/csv content type.
    /// </summary>
    public static HttpContentType TextCsv => new("text", "csv", new NameValueCollection());
    
    /// <summary>
    /// Creates an application/json content type.
    /// </summary>
    public static HttpContentType ApplicationJson => new("application", "json", new NameValueCollection());
    
    /// <summary>
    /// Creates an application/xml content type.
    /// </summary>
    public static HttpContentType ApplicationXml => new("application", "xml", new NameValueCollection());
    
    /// <summary>
    /// Creates an application/octet-stream content type.
    /// </summary>
    public static HttpContentType ApplicationOctetStream => new("application", "octet-stream", new NameValueCollection());
    
    /// <summary>
    /// Creates an application/x-www-form-urlencoded content type.
    /// </summary>
    public static HttpContentType ApplicationFormUrlEncoded => new("application", "x-www-form-urlencoded", new NameValueCollection());
    
    /// <summary>
    /// Creates a multipart/form-data content type.
    /// </summary>
    public static HttpContentType MultipartFormData => new("multipart", "form-data", new NameValueCollection());
    
}