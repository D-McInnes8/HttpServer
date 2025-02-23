using System.Collections.Specialized;
using System.Text;
using HttpServer.Headers;
using HttpServer.Networking;
using HttpServer.Response.Body;
using HttpServer.Routing;

namespace HttpServer.Request.Parser;

/// <summary>
/// Static class containing methods to parse a HTTP request.
/// </summary>
public static class HttpRequestParser
{
    /// <summary>
    /// Parses a HTTP request from a <see cref="INetworkStreamReader"/>.
    /// </summary>
    /// <param name="networkStreamReader">The network stream reader to parse the request from.</param>
    /// <returns>The parsed HTTP request.</returns>
    public static async Task<Result<HttpRequest, string>> Parse(INetworkStreamReader networkStreamReader)
    {
        var requestLine = await networkStreamReader.ReadLineAsync();
        
        if (string.IsNullOrWhiteSpace(requestLine))
        {
            return Result.Error<HttpRequest, string>("Unable to parse request line.");
        }
        
        var tokenizer = new StringTokenizer(requestLine, [' ']);
        var method = ParseMethod(tokenizer.GetNextToken());
        var path = tokenizer.GetNextToken();
        var httpVersion = tokenizer.GetNextToken();
        
        if (method == HttpRequestMethod.UNKNOWN)
        {
            return Result.Error<HttpRequest, string>("Unknown HTTP method.");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Error<HttpRequest, string>("Invalid path.");
        }

        if (string.IsNullOrWhiteSpace(httpVersion))
        {
            return Result.Error<HttpRequest, string>("Invalid HTTP version.");
        }
        
        var headers = new NameValueCollection();
        string? line;
        while (!string.IsNullOrWhiteSpace(line = await networkStreamReader.ReadLineAsync()))
        {
            if (TryGetParsedHeader(line, out var httpHeader))
            {
                headers.Add(httpHeader.Key, httpHeader.Value);
            }
        }

        HttpContentType? httpContentType = null;
        if (headers["Content-Type"] is not null
            && HttpContentType.TryParse(headers["Content-Type"], out var contentType))
        {
            httpContentType = contentType;
        }
        
        var contentLength = headers["Content-Length"];
        var body = contentLength is not null ? await networkStreamReader.ReadBytesAsync(int.Parse(contentLength)) : null;
        if (body is null || (body.Length == 0 && httpContentType is null))
        {
            return new HttpRequest(method, path)
            {
                Headers = headers,
                HttpVersion = httpVersion,
                ContentType = httpContentType,
            };
        }

        if (httpContentType is null)
        {
            return Result.Error<HttpRequest, string>("Content-Type header is required.");
        }
        var encoding = httpContentType.Charset is not null ? Encoding.GetEncoding(httpContentType.Charset) : Encoding.UTF8;
        return new HttpRequest(method, path)
        {
            Headers = headers,
            Body = CreateBodyContent(body, httpContentType, encoding),
            HttpVersion = httpVersion,
            ContentType = httpContentType,
        };
    }
    
    /// <summary>
    /// Parses the path of a HTTP request into a route and a collection of query parameters.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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
        
        if (query.Length == 0 || query[0] == '\0')
        {
            return (pathString, parameters);
        }
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
    
    private static HttpBodyContent CreateBodyContent(byte[] body, HttpContentType contentType, Encoding encoding)
    {
        if (HttpContentType.TextPlain.Equals(contentType))
        {
            return new StringBodyContent(encoding.GetString(body), contentType, encoding);
        }
        
        return new ByteArrayBodyContent(body, contentType, encoding);
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