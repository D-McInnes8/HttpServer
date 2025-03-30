using System.Diagnostics.CodeAnalysis;

namespace HttpServer.Headers;

/// <summary>
/// Represents the value of the Accept-Encoding header.
/// </summary>
public class AcceptEncoding : IHttpHeader, ISpanParsable<AcceptEncoding>
{
    /// <summary>
    /// The encodings accepted by the client.
    /// </summary>
    public string[] Encodings { get; private set; }
    
    /// <summary>
    /// Constructs a new <see cref="AcceptEncoding"/> with no encodings.
    /// </summary>
    public AcceptEncoding()
    {
        Encodings = [];
    }
    
    /// <summary>
    /// Constructs a new <see cref="AcceptEncoding"/> with the specified encodings.
    /// </summary>
    /// <param name="encodings">The encodings accepted by the client.</param>
    public AcceptEncoding(params string[] encodings)
    {
        ArgumentNullException.ThrowIfNull(encodings);
        Encodings = encodings;
    }
    
    /// <inheritdoc />
    public string Render() => string.Join(", ", Encodings);

    /// <inheritdoc />
    public static AcceptEncoding Parse(string s, IFormatProvider? provider)
    {
        return Parse(s.AsSpan(), provider);
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AcceptEncoding result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <inheritdoc />
    public static AcceptEncoding Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        var values = s.Split(',');
        var encodings = new List<string>();
        foreach (var index in values)
        {
            var encoding = s[index].Trim().ToString();
            encodings.Add(encoding);
        }
        
        return new AcceptEncoding(encodings.ToArray());
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out AcceptEncoding result)
    {
        var values = s.Split(',');
        var encodings = new List<string>(capacity: 3);
        foreach (var index in values)
        {
            var encoding = s[index].Trim().ToString();
            encodings.Add(encoding);
        }
        
        result = new AcceptEncoding(encodings.ToArray());
        return true;
    }
}