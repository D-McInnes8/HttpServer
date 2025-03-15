using System.Diagnostics.CodeAnalysis;

namespace HttpServer.Headers;

public class ContentDisposition : IHttpHeader, ISpanParsable<ContentDisposition>
{
    public string? FileName { get; internal set; }
    public string? Name { get; internal set; }

    public ContentDisposition()
    {
    }

    public ContentDisposition(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
    }
    
    public ContentDisposition(string fileName, string name)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(name);
        
        FileName = fileName;
        Name = name;
    }

    public static ContentDisposition Parse(string s, IFormatProvider? provider)
    {
        return Parse(s.AsSpan(), provider);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ContentDisposition result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    public static ContentDisposition Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out ContentDisposition result)
    {
        result = new ContentDisposition();
        
        var delimiterIndex = s.IndexOf(';');
        if (delimiterIndex == -1)
        {
            return true;
        }
        
        var type = s.Slice(0, delimiterIndex);
        
        // Parse header parameters
        var parameters = s.Slice(delimiterIndex + 1);
        var parameterPairs = parameters.Split(';');
        foreach (var parameterPair in parameterPairs)
        {
            var slice = parameters[parameterPair.Start .. parameterPair.End];
            var parameterDelimiterIndex = slice.IndexOf('=');

            if (parameterDelimiterIndex != -1)
            {
                var paramName = slice[..parameterDelimiterIndex].TrimStart();
                var paramValue = slice[(parameterDelimiterIndex + 1)..];
                if (paramName.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    result.Name = paramValue.ToString();
                }

                if (paramName.Equals("filename", StringComparison.OrdinalIgnoreCase))
                {
                    result.FileName = paramValue.ToString();
                }
            }
        }
        
        return true;
    }

    public string Render()
    {
        return $"form-data; name={Name}; filename={FileName}";
    }
}