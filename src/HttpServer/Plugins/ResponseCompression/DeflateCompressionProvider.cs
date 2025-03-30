using System.IO.Compression;

namespace HttpServer.Plugins.ResponseCompression;

public class DeflateCompressionProvider : ICompressionProvider
{
    public string EncodingName => "deflate";
    
    public Stream GetCompressionStream(Stream responseStream)
    {
        return new DeflateStream(responseStream, CompressionMode.Compress);
    }
}