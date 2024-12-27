using System.Collections.Specialized;
using System.Diagnostics;

namespace Application.Request.Parser;

public ref struct SpanReader
{
    private ReadOnlySpan<char> _s;
    private int _pos;
    
    public SpanReader(ReadOnlySpan<char> s)
    {
        _s = s;
        _pos = 0;
    }

    public ReadOnlySpan<char> ReadLine()
    {
        for (var i = _pos; i < _s.Length; i++)
        {
            if (i < _s.Length - 1
                && _s[i] == '\r' && _s[i + 1] == '\n')
            {
                var line = _s[_pos..i];
                _pos = i + 2;
                return line;
            }

            if (_s[i] == '\r' || _s[i] == '\n')
            {
                var line = _s[_pos..i];
                _pos = i + 1;
                return line;
            }
        }

        var lastLine = _s[_pos..];
        _pos = _s.Length;
        return lastLine;
    }

    public ReadOnlySpan<char> ReadToEnd()
    {
        return _s[_pos..];
    }
}

public static class HttpRequestParser
{
    public static HttpRequest Parse(ReadOnlySpan<char> request)
    {
        var spanReader = new SpanReader(request);
        var requestLine = spanReader.ReadLine(); //.ToString();

        var tokenizer = new StringTokenizer(requestLine, [' ']);
        var method = ParseMethod(tokenizer[0]);
        var path = tokenizer[1].ToString();
        var httpVersion = tokenizer[2].ToString();

        var headers = new Dictionary<string, string>();

        ReadOnlySpan<char> line;
        do
        {
            line = spanReader.ReadLine();
            _ = TryGetParsedHeader(line, out var httpHeader);
            headers.Add(httpHeader.Key, httpHeader.Value);
        }
        while (line.Length > 0 && line is not "\r\n" && line is not "\r" && line is not "\n");

        var body = spanReader.ReadToEnd().ToString();

        return new HttpRequest(method, path)
        {
            Headers = headers,
            Body = string.IsNullOrWhiteSpace(body) ? null : body,
            HttpVersion = httpVersion,
            ContentType = headers.GetValueOrDefault("Content-Type"),
        };
    }
    
    public static (string Path, NameValueCollection Parameters) ParsePath(ReadOnlySpan<char> path)
    {
        var parameters = new NameValueCollection();
        var queryIndex = path.IndexOf('?');
        if (queryIndex == -1)
        {
            return (path.ToString(), parameters);
        }
        
        var pathString = path[..queryIndex].ToString();
        var query = path[(queryIndex + 1)..];
        
        var queryTokenizer = new StringTokenizer(query, ['&']);
        foreach (var range in queryTokenizer.Tokens)
        {
            var queryParam = query[range.Start..range.End];
            var delimiterIndex = queryParam.IndexOf('=');
            if (delimiterIndex != -1)
            {
                var key = queryParam[..delimiterIndex].ToString();
                var value = queryParam[(delimiterIndex + 1)..].ToString();
                parameters.Add(key, value);
            }
        }

        return (pathString, parameters);
    }
    
    private static bool TryGetParsedHeader(ReadOnlySpan<char> header, out KeyValuePair<string, string> httpHeader)
    {
        var delimiterIndex = header.IndexOf(':');
        if (delimiterIndex != -1)
        {
            var key = header[..delimiterIndex].ToString();
            var value = header[(delimiterIndex + 2)..].ToString();
            httpHeader = new KeyValuePair<string, string>(key, value);
            return true;
        }
        
        httpHeader = new KeyValuePair<string, string>(string.Empty, string.Empty);
        return false;
    }
    
    private static HttpRequestMethod ParseMethod(ReadOnlySpan<char> method)
    {
        return method switch
        {
            "GET" => HttpRequestMethod.GET,
            "POST" => HttpRequestMethod.POST,
            "PUT" => HttpRequestMethod.PUT,
            "DELETE" => HttpRequestMethod.DELETE,
            "PATCH" => HttpRequestMethod.PATCH,
            "HEAD" => HttpRequestMethod.HEAD,
            "OPTIONS" => HttpRequestMethod.OPTIONS,
            "CONNECT" => HttpRequestMethod.CONNECT,
            "TRACE" => HttpRequestMethod.TRACE,
            _ => HttpRequestMethod.UNKNOWN,
        };
    }
}