using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Application;

public readonly struct HttpContentType : IParsable<HttpContentType>
{
    public required string Type { get; init; } = "text";
    public required string SubType { get; init; } = "plain";
    public required NameValueCollection Parameters { get; init; }
    public string? Charset => Parameters["charset"];
    public string? Boundary => Parameters["boundary"];

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
    
    [SetsRequiredMembers]
    public HttpContentType(string type, string subType, NameValueCollection parameters)
    {
        Type = type;
        SubType = subType;
        Parameters = parameters;
    }

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

    public static HttpContentType Parse(string s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new ArgumentException("HttpContent value cannot be null or whitespace.", nameof(s));
        }

        return Parse(s);
    }

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

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out HttpContentType result)
    {
        return TryParse(s, out result);
    }
    
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
                parametersCollection.Add(parameterKey.ToString(), parameterValue.ToString());
            }
        }
        
        result = new HttpContentType(type, subType, parametersCollection);
        return true;
    }

    public override string ToString() => $"{Type}/{SubType}";
}