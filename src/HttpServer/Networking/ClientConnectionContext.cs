using System.Net.Sockets;

namespace HttpServer.Networking;

/// <summary>
/// Represents the context of a client connection.
/// </summary>
public class ClientConnectionContext
{
    /// <summary>
    /// The request reader for the client connection.
    /// </summary>
    public INetworkStreamReader RequestReader { get; }
    
    /// <summary>
    /// The writer to be used to write the response to the network stream.
    /// </summary>
    public INetworkStreamWriter ResponseWriter { get; }

    /// <summary>
    /// Constructs a new <see cref="ClientConnectionContext"/> with the specified network stream.
    /// </summary>
    /// <param name="networkStream">The network stream to be used for the connection.</param>
    public ClientConnectionContext(NetworkStream networkStream)
    {
        ArgumentNullException.ThrowIfNull(networkStream);
        RequestReader = new TcpNetworkStreamReader(networkStream);
        ResponseWriter = new TcpNetworkStreamWriter(networkStream);
    }
}