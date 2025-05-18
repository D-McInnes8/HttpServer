using System.IO.Compression;

namespace HttpServer.Plugins.ResponseCompression;

/// <summary>
/// A compression provider for GZip.
/// </summary>
public class GZipCompressionProvider : ICompressionProvider
{
    public string EncodingName => "gzip";

    public Stream GetCompressionStream(Stream responseStream)
    {
        return new GZipStream(responseStream, CompressionMode.Compress, leaveOpen: true);
    }
}