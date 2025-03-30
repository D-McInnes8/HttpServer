namespace HttpServer.Plugins.ResponseCompression;

/// <summary>
/// A provider for the stream to compress the response body.
/// </summary>
public interface ICompressionProvider
{
    /// <summary>
    /// Gets the name of the compression encoding. Will be used in the "Content-Encoding" header.
    /// </summary>
    public string EncodingName { get; }
    
    /// <summary>
    /// Gets the compression stream.
    /// </summary>
    /// <param name="responseStream">The original response stream to output the compressed response to.</param>
    /// <returns>The compression stream.</returns>
    public Stream GetCompressionStream(Stream responseStream);
}