using System.Text;

namespace HttpServer.Response.Internal;

/// <summary>
/// Contains methods to write an <see cref="HttpResponse"/> to a string.
/// </summary>
public static class HttpResponseWriter
{
    /// <summary>
    /// Writes an <see cref="HttpResponse"/> to a string.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponse"/> object to write.</param>
    /// <returns>The <see cref="HttpResponse"/> object serialised into a proper HTTP response.</returns>
    public static byte[] WriteResponse(HttpResponse response)
    {
        if (response.Body is not null)
        {
            response.Body.ContentType.Charset = response.Body.Encoding.WebName;
            response.Headers["Content-Type"] = response.Body.ContentType.Render();
            response.Headers["Content-Length"] = response.Body.Content.Length.ToString();
        }
        
        var metadata = WriteMetadata(response);
        if (response.Body is null)
        {
            return metadata;
        }
        
        var httpResponseBytes = new byte[metadata.Length + response.Body.Content.Length];
        metadata.CopyTo(httpResponseBytes, 0);
        response.Body.Content.CopyTo(httpResponseBytes, metadata.Length);
        return httpResponseBytes;
    }

    private static byte[] WriteMetadata(HttpResponse response)
    {
        var sb = new StringBuilder();
        sb.Append($"{response.HttpVersion} {(int)response.StatusCode} {response.StatusCode}\r\n");
        
        foreach (var (key, value) in response.Headers)
        {
            sb.Append($"{key}: {value}\r\n");
        }
        sb.Append("\r\n");

        var responseMetadata = sb.ToString();
        return Encoding.ASCII.GetBytes(responseMetadata);
    }
}