using System.IO.Compression;

namespace HttpServer.Plugins.ResponseCompression;

public class GZipCompressionProvider : ICompressionProvider
{
    public string EncodingName => "gzip";

    public Stream GetCompressionStream(Stream responseStream)
    {
        return new GZipStream(responseStream, CompressionMode.Compress);
    }
}