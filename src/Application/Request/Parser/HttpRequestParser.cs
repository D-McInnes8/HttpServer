namespace Application.Request.Parser;

public static class HttpRequestParser
{
    public static HttpRequest Parse(ReadOnlySpan<char> request)
    {
        for (int i = 0; i < request.Length; i++)
        {
            if (request[i] == '\r' && request[i + 1] == '\n' && request[i + 2] == '\r' && request[i + 3] == '\n')
            {
                var header = request[..i];
                var body = request[(i + 4)..];
                var (method, path) = ParseHeader(header);
                return new HttpRequest
                {
                    Method = method,
                    Path = path,
                    Body = body.Length > 0 ? body.ToString() : null,
                    Headers = new Dictionary<string, string>()
                };
            }
        }

        throw new InvalidOperationException();
    }
    
    private static (HttpRequestMethod, string) ParseHeader(ReadOnlySpan<char> header)
    {
        var parts = header.Split(' ');
        
        HttpRequestMethod method = HttpRequestMethod.UNKNOWN;
        string? path = null;
        int count = 0;
        
        foreach (var part in parts)
        {
            if (count == 0)
            {
                method = ParseMethod(header[part.Start.Value..part.End.Value]);
            }
            else if (count == 1)
            {
                path = header[part.Start.Value..part.End.Value].ToString();
            }

            count++;
        }
        //var method = ParseMethod(parts.;
        //var path = parts[1].ToString();
        
        return (method, path);
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