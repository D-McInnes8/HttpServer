using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace HttpServer.Networking;

/// <summary>
/// Represents the state of a <see cref="TcpClient"/> connection.
/// </summary>
public class TcpClientConnection
{
    /// <summary>
    /// The client that is connected to the server.
    /// </summary>
    public required TcpClient Client { get; set; }
    
    /// <summary>
    /// Whether the connection has been disposed.
    /// </summary>
    public bool IsDisposed { get; set; }
    
    /// <summary>
    /// The last time a request was received from the client.
    /// </summary>
    public DateTime? LastRequest { get; set; }
    
    /// <summary>
    /// The time the connection was opened.
    /// </summary>
    public required DateTime ConnectionOpened { get; set; }
    
    /// <summary>
    /// The number of requests received from the client.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// The stream for the client connection.
    /// </summary>
    public Stream Stream => Client.GetStream();

    /// <summary>
    /// Initializes a new instance of <see cref="TcpClientConnection"/>.
    /// </summary>
    /// <param name="tcpClient"></param>
    /// <param name="connectionOpened"></param>
    [SetsRequiredMembers]
    public TcpClientConnection(TcpClient tcpClient, DateTime connectionOpened)
    {
        Client = tcpClient;
        ConnectionOpened = connectionOpened;
    }
}