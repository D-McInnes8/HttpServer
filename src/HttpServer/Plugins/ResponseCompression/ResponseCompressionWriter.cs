using System.IO.Pipelines;
using HttpServer.Networking;

namespace HttpServer.Plugins.ResponseCompression;

/// <summary>
/// A writer for the response compression.
/// </summary>
public class ResponseCompressionWriter : INetworkStreamWriter, IAsyncDisposable
{
    public Stream Stream { get; }
    public PipeWriter PipeWriter { get; }
    public MemoryStream MemoryStream { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseCompressionWriter"/> class.
    /// </summary>
    /// <param name="networkStreamWriter">The existing network stream writer.</param>
    /// <param name="compressionProvider">The compression provider to provide the compression stream.</param>
    public ResponseCompressionWriter(INetworkStreamWriter networkStreamWriter, ICompressionProvider compressionProvider)
    {
        //var innerStream = networkStreamWriter.Stream;
        MemoryStream = new MemoryStream();
        Stream = compressionProvider.GetCompressionStream(MemoryStream);
        PipeWriter = PipeWriter.Create(Stream);
    }

    public async ValueTask DisposeAsync()
    {
        await Stream.DisposeAsync();
        await MemoryStream.DisposeAsync();
    }
}