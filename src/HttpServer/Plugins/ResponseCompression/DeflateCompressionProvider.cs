using System.IO.Compression;

namespace HttpServer.Plugins.ResponseCompression;

/// <summary>
/// A compression provider for the deflate algorithm.
/// </summary>
public class DeflateCompressionProvider : ICompressionProvider
{
    public string EncodingName => "deflate";
    
    public Stream GetCompressionStream(Stream responseStream)
    {
        return new DeflateStream(responseStream, CompressionMode.Compress, leaveOpen: false);
    }
}