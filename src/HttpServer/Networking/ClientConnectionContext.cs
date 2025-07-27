using System.Net.Sockets;

namespace HttpServer.Networking;

/// <summary>
/// Represents the context of a client connection.
/// </summary>
public class ClientConnectionContext : IDisposable
{
    /// <summary>
    /// The request reader for the client connection.
    /// </summary>
    public INetworkStreamReader RequestReader { get; }
    
    /// <summary>
    /// TThe writer to be used to write the response to the network stream.
    /// </summary>
    public INetworkStreamWriter ResponseWriter { get; }
    
    /// <summary>
    /// The writer to be used to write the response body to the network stream.
    /// </summary>
    public INetworkStreamWriter ResponseBodyWriter { get; set; }
    
    public Stream? ResponseBodyWriter2 { get; set; }
    
    /// <summary>
    /// The inner memory stream used to write the response body.
    /// This is used to buffer the response body before sending it over the network.
    /// </summary>
    public MemoryStream ResponseMemoryStream { get; set; }

    /// <summary>
    /// Constructs a new <see cref="ClientConnectionContext"/> with the specified network stream.
    /// </summary>
    /// <param name="networkStream">The network stream to be used for the connection.</param>
    public ClientConnectionContext(NetworkStream networkStream)
    {
        ArgumentNullException.ThrowIfNull(networkStream);
        RequestReader = new TcpNetworkStreamReader(networkStream);
        ResponseWriter = new TcpNetworkStreamWriter(networkStream);
        ResponseBodyWriter = new TcpNetworkStreamWriter(networkStream);
        ResponseMemoryStream = new MemoryStream();
    }

    public void Dispose()
    {
        ResponseMemoryStream.Dispose();
    }
}