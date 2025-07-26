using System.IO.Compression;

namespace HttpServer.Plugins.ResponseCompression;

/// <summary>
/// A compression provider for the brotli algorithm.
/// </summary>
public class BrotliCompressionProvider : ICompressionProvider
{
    public string EncodingName => "br";
    
    public Stream GetCompressionStream(Stream responseStream)
    {
        return new BrotliStream(responseStream, CompressionMode.Compress, leaveOpen: false);
    }
}