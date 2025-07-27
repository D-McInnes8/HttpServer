using System.Buffers;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using HttpServer.Networking;
using HttpServer.Pipeline;

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
    public static byte[] WriteResponse(HttpResponse response, INetworkStreamWriter responseBodyWriter)
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
        
        var httpResponseBytes = new byte[metadata.Length + response.Body.Length];
        metadata.CopyTo(httpResponseBytes, 0);
        response.Body.CopyTo(httpResponseBytes.AsSpan(metadata.Length));
        return httpResponseBytes;
    }

    private static async Task WriteResponseWithBodyAsync(HttpResponse response, ClientConnectionContext ctx)
    {
        if (response.Body is null)
        {
            throw new InvalidOperationException("Response body cannot be null when writing response with body.");
        }
        
        var buffer = response.Body.ToArray();
        // var bytes = Convert.FromBase64String("H4sIAAAAAAAAA3POzy0oSi0uTk1RBAAsG2/iCwAAAA==");
        //
        // if (buffer.Length < 2 || buffer[0] != 0x1F || buffer[1] != 0x8B)
        // {
        //     throw new InvalidOperationException("Input buffer is not in gzip format.");
        // }
        //
        // // Write compressed data to memory stream
        // await using var compressedStream = new MemoryStream(buffer);
        //
        // // Create GZipStream for decompression
        // await using var gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        //
        // // Read decompressed data into another memory stream
        // await using var decompressedStream = new MemoryStream();
        // await gZipStream.CopyToAsync(decompressedStream);
        // decompressedStream.Seek(0, SeekOrigin.Begin);
        //
        // var uncompressed = decompressedStream.ToArray();
        // var resultString = Encoding.ASCII.GetString(uncompressed);
        
        // Ensure the content type and length are set correctly
        response.Body.ContentType.Charset = response.Body.Encoding.WebName;
        response.Headers["Content-Type"] = response.Body.ContentType.Render();
        response.Headers["Content-Length"] = buffer.Length.ToString();
        
        // Write the response metadata to the PipeWriter
        var metadata = WriteMetadata(response);
        var metadataMemory = ctx.ResponseWriter.PipeWriter.GetMemory(metadata.Length);
        metadata.CopyTo(metadataMemory);
        ctx.ResponseWriter.PipeWriter.Advance(metadata.Length);
        
        // Write the body content to the PipeWriter
        var bodyMemory = ctx.ResponseWriter.PipeWriter.GetSpan(buffer.Length);
        buffer.CopyTo(bodyMemory);
        ctx.ResponseWriter.PipeWriter.Advance(buffer.Length);
        
        _ = await ctx.ResponseWriter.PipeWriter.FlushAsync();
    }

    private static async Task WriteResponseWithoutBodyAsync(HttpResponse response, ClientConnectionContext ctx)
    {
        var metadata = WriteMetadata(response);
        var metadataMemory = ctx.ResponseWriter.PipeWriter.GetMemory(metadata.Length);
        metadata.CopyTo(metadataMemory);
        ctx.ResponseWriter.PipeWriter.Advance(metadata.Length);
        
        _ = await ctx.ResponseWriter.PipeWriter.FlushAsync();
    }
    
    public static async Task WriteResponseAsync(HttpResponse response, ClientConnectionContext ctx)
    {
        if (response.Body is null)
        {
            await WriteResponseWithoutBodyAsync(response, ctx);
            return;
        }
        
        await WriteResponseWithBodyAsync(response, ctx);
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