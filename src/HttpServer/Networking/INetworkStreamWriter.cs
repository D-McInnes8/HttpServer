using System.IO.Pipelines;

namespace HttpServer.Networking;

/// <summary>
/// An interface for writing HTTP responses to the underlying network stream.
/// </summary>
public interface INetworkStreamWriter
{
    /// <summary>
    /// The stream to write to.
    /// </summary>
    public Stream Stream { get; }
    
    /// <summary>
    /// The pipe writer wrapper around the stream.
    /// </summary>
    public PipeWriter PipeWriter { get; }
}