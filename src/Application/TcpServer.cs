using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Application;

/// <summary>
/// The TCP server that listens for incoming TCP requests.
/// </summary>
internal class TcpServer
{
    /// <summary>
    /// The port the server is listening on.
    /// </summary>
    public int Port { get; private set; }
    
    /// <summary>
    /// The local endpoint the server is listening on.
    /// </summary>
    public Uri LocalEndpoint => new Uri(_tcpListener.LocalEndpoint.ToString()!);
    
    private readonly TcpListener _tcpListener;
    private bool _isRunning;
    
    private readonly Func<string, string> _requestHandler;

    /// <summary>
    /// Creates a new <see cref="TcpServer"/> with the specified port and request handler.
    /// </summary>
    /// <param name="port">The port the TCP server will listen on.</param>
    /// <param name="requestHandler">The request handler to execute when receiving a TCP request.</param>
    public TcpServer(int port, Func<string, string> requestHandler)
    {
        Port = port;
        _requestHandler = requestHandler;
        _tcpListener = new TcpListener(IPAddress.Any, Port);
    }
    
    /// <summary>
    /// Starts the TCP server.
    /// </summary>
    /// <returns></returns>
    public Task StartAsync()
    {
        _isRunning = true;
        _tcpListener.Start();
        Console.WriteLine($"Listening on port {_tcpListener.LocalEndpoint}");

        var thread = new Thread(ListenAsync);
        thread.Start();
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the TCP server.
    /// </summary>
    /// <returns></returns>
    public Task StopAsync()
    {
        _isRunning = false;
        _tcpListener.Stop();
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Listens for incoming TCP requests asynchronously.
    /// </summary>
    private void ListenAsync()
    {
        while (_isRunning)
        {
            try
            {
                var client = _tcpListener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleRequest, client);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                Console.WriteLine("Socket interrupted");
            }
        }
        
        Console.WriteLine("Server stopped");
    }

    /// <summary>
    /// Handles a TCP request and forwards the request to the HTTP server.
    /// </summary>
    /// <param name="state">The <see cref="TcpClient"/> state object passed to the handler by the <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object?)"/> function.</param>
    private void HandleRequest(object? state)
    {
        if (state is not TcpClient client)
        {
            return;
        }
        
        using var stream = client.GetStream();
        
        var buffer = new byte[2048];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);
        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        try
        {
            var response = _requestHandler(message);
            stream.Write(Encoding.UTF8.GetBytes(response));
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("An uncaught exception occurred in the request worker thread: " + ex.Message);
        }
        
        /*var body = "Hello World";
        var responseMessage = $"""
                               HTTP/1.1 200 OK
                               Date: {DateTime.UtcNow:R}
                               Content-Length: {body.Length}
                               Content-Type: text/plain

                               {body}
                               """;*/
    }
}