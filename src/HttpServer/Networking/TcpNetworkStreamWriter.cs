using System.IO.Pipelines;
using System.Net.Sockets;

namespace HttpServer.Networking;

/// <summary>
/// An implementation of <see cref="INetworkStreamWriter"/> for writing to a <see cref="NetworkStream"/>.
/// </summary>
internal class TcpNetworkStreamWriter : INetworkStreamWriter
{
    /// <summary>
    /// The <see cref="NetworkStream"/> to write to.
    /// </summary>
    public Stream Stream { get; }
    
    /// <summary>
    /// The <see cref="PipeWriter"/> wrapper around the <see cref="NetworkStream"/>.
    /// </summary>
    public PipeWriter PipeWriter { get; }
    
    /// <summary>
    /// Creates a new <see cref="TcpNetworkStreamWriter"/> with the specified <see cref="NetworkStream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="NetworkStream"/> to write to.</param>
    public TcpNetworkStreamWriter(NetworkStream stream)
    {
        Stream = stream;
        PipeWriter = PipeWriter.Create(stream);
    }
}