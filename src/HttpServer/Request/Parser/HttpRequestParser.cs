using System.Collections.Specialized;

namespace HttpServer.Request.Parser;

public static class HttpRequestParser
{
    public static async Task<HttpRequest> Parse(Stream stream)
    {
        using var reader = new HttpRequestStreamReader(stream);
        var requestLine = await reader.ReadLineAsync();
        var tokenizer = new StringTokenizer(requestLine, [' ']);
        var method = ParseMethod(tokenizer[0]);
        var path = tokenizer[1].ToString();
        var httpVersion = tokenizer[2].ToString();
        
        var headers = new Dictionary<string, string>();
        string? line;
        while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
        {
            _ = TryGetParsedHeader(line, out var httpHeader);
            headers.Add(httpHeader.Key, httpHeader.Value);
        }

        HttpContentType? httpContentType = null;
        if (headers.TryGetValue("Content-Type", out var headerContentType)
            && HttpContentType.TryParse(headerContentType, out var contentType))
        {
            httpContentType = contentType;
        }
        
        var contentLength = headers.GetValueOrDefault("Content-Length");
        var body = await reader.ReadToEndAsync(contentLength is not null ? int.Parse(contentLength) : 0);
        return new HttpRequest(method, path)
        {
            Headers = headers,
            Body = string.IsNullOrWhiteSpace(body) ? null : body,
            HttpVersion = httpVersion,
            ContentType = httpContentType,
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