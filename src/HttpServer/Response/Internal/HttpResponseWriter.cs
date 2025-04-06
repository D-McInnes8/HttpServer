using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using HttpServer.Networking;

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
        
        response.BodyStream.Write(response.Body.Content, 0, response.Body.Length);
        response.BodyStream.Flush();
        
        var httpResponseBytes = new byte[metadata.Length + response.BodyStream.Length];
        metadata.CopyTo(httpResponseBytes, 0);
        //response.Body.CopyTo(httpResponseBytes.AsSpan(metadata.Length));
        response.BodyStream.Position = 0;
        var bytesRead = response.BodyStream.Read(httpResponseBytes.AsSpan(metadata.Length));
        return httpResponseBytes;
    }
    
    public static async Task WriteResponseAsync(HttpResponse response, PipeWriter pipeWriter)
    {
        if (response.Body is not null)
        {
            response.Body.ContentType.Charset = response.Body.Encoding.WebName;
            response.Headers["Content-Type"] = response.Body.ContentType.Render();
            response.Headers["Content-Length"] = response.Body.Content.Length.ToString();
        }
        
        var metadata = WriteMetadata(response);
        var metadataMemory = pipeWriter.GetMemory(metadata.Length);
        metadata.CopyTo(metadataMemory);
        pipeWriter.Advance(metadata.Length);
        
        if (response.Body is not null)
        {
            var responseLength = response.Body.Length;
            var bodySpan = pipeWriter.GetSpan(responseLength);
            response.Body.CopyTo(bodySpan);
            pipeWriter.Advance(responseLength);
        }
        
        _ = await pipeWriter.FlushAsync();
    }
    
    public static void CopyTo(NetworkStream destination)
    {
        
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