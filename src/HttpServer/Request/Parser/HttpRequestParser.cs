using System.Collections.Specialized;
using System.Text;
using HttpServer.Body;
using HttpServer.Body.Serializers;
using HttpServer.Headers;
using HttpServer.Networking;
using HttpServer.Routing;

namespace HttpServer.Request.Parser;

/// <summary>
/// Static class containing methods to parse a HTTP request.
/// </summary>
public class HttpRequestParser
{
    private readonly IHttpBodyContentSerializerProvider _serializerProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpRequestParser"/>.
    /// </summary>
    /// <param name="serializerProvider">The serializer provider to use for deserializing the request body.</param>
    public HttpRequestParser(IHttpBodyContentSerializerProvider serializerProvider)
    {
        _serializerProvider = serializerProvider;
    }

    /// <summary>
    /// Parses a HTTP request from a <see cref="INetworkStreamReader"/>.
    /// </summary>
    /// <param name="networkStreamReader">The network stream reader to parse the request from.</param>
    /// <returns>The parsed HTTP request.</returns>
    public async Task<Result<HttpRequest, string>> Parse(INetworkStreamReader networkStreamReader)
    {
        var requestLine = await networkStreamReader.ReadLineAsync();

        HttpParserException.ThrowIfNullOrWhiteSpace(requestLine, HttpParserExceptionErrorCode.InvalidRequestLine);
        var tokenizer = new StringTokenizer(requestLine, [' ']);
        var method = ParseMethod(tokenizer.GetNextToken());
        var path = tokenizer.GetNextToken();
        var httpVersion = tokenizer.GetNextToken();
        
        if (method == HttpRequestMethod.UNKNOWN)
        {
            throw new HttpParserException(HttpParserExceptionErrorCode.InvalidMethod);
        }
        HttpParserException.ThrowIfNullOrWhiteSpace(path, HttpParserExceptionErrorCode.InvalidUri);
        HttpParserException.ThrowIfNullOrWhiteSpace(httpVersion, HttpParserExceptionErrorCode.InvalidHttpVersion);
        
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
        
        AcceptEncoding? acceptEncoding = null;
        if (headers["Accept-Encoding"] is not null
            && AcceptEncoding.TryParse(headers["Accept-Encoding"], null, out var acceptEncodingHeader))
        {
            acceptEncoding = acceptEncodingHeader;
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
                AcceptEncoding = acceptEncoding,
            };
        }

        HttpParserException.ThrowIfNull(httpContentType, HttpParserExceptionErrorCode.InvalidContentType);
        var encoding = httpContentType.Charset is not null ? Encoding.GetEncoding(httpContentType.Charset) : Encoding.ASCII;
        return new HttpRequest(method, path)
        {
            Headers = headers,
            Body = CreateBodyContent(body, httpContentType, encoding),
            HttpVersion = httpVersion,
            ContentType = httpContentType,
            AcceptEncoding = acceptEncoding,
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
    
    private HttpBodyContent CreateBodyContent(byte[] body, HttpContentType contentType, Encoding encoding)
    {
        var deserializer = _serializerProvider.GetSerializer(contentType);
        return deserializer.Deserialize(body, contentType, encoding);
        
        /*if (HttpContentType.TextPlain.Equals(contentType))
        {
            return new StringBodyContent(encoding.GetString(body), contentType, encoding);
        }
        
        return new ByteArrayBodyContent(body, contentType, encoding);*/
    }
    
    public static bool TryGetParsedHeader(ReadOnlySpan<char> header, out KeyValuePair<string, string> httpHeader)
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