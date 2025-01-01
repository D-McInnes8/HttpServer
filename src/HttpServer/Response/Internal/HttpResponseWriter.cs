using System.Text;

namespace HttpServer.Response.Internal;

public class HttpResponseWriter
{
    public static string WriteResponse(HttpResponse response)
    {
        if (response.Body != null)
        {
            response.Headers["Content-Type"] = response.Body.ContentType;
            response.Headers["Content-Length"] = response.Body?.Content.Length.ToString() ?? "0";
        }
        
        var sb = new StringBuilder();
        
        sb.Append($"{response.HttpVersion} {(int)response.StatusCode} {response.StatusCode}\r\n");
        
        foreach (var (key, value) in response.Headers)
        {
            sb.Append($"{key}: {value}\r\n");
        }
        sb.Append("\r\n");
        
        if (response.Body != null)
        {
            sb.Append(response.Body.Content);
        }

        return sb.ToString();
    }
}