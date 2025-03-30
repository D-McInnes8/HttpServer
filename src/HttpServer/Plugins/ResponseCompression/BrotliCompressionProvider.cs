using System.IO.Compression;

namespace HttpServer.Plugins.ResponseCompression;

public class BrotliCompressionProvider : ICompressionProvider
{
    public string EncodingName => "br";
    
    public Stream GetCompressionStream(Stream responseStream)
    {
        return new BrotliStream(responseStream, CompressionMode.Compress);
    }
}