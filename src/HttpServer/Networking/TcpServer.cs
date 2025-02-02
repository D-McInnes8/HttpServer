using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace HttpServer.Networking;

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
    private HttpWebServerOptions _options;
    private readonly ILogger<TcpServer> _logger;

    private readonly Func<Stream, byte[]> _requestHandler;
    private IConnectionPool _connectionPool;

    /// <summary>
    /// Creates a new <see cref="TcpServer"/> with the specified port and request handler.
    /// </summary>
    /// <param name="port">The port the TCP server will listen on.</param>
    /// <param name="requestHandler">The request handler to execute when receiving a TCP request.</param>
    /// <param name="logger">The <see cref="ILogger"/> object to be logged to.</param>
    /// <param name="connectionPool">The <see cref="IConnectionPool"/> object to manage connections.</param>
    /// <param name="options">The <see cref="HttpWebServerOptions"/> object containing the server options.</param>
    public TcpServer(int port, Func<Stream, byte[]> requestHandler, ILogger<TcpServer> logger, IConnectionPool connectionPool, HttpWebServerOptions options)
    {
        Port = port;
        _requestHandler = requestHandler;
        _logger = logger;
        _options = options;
        _tcpListener = new TcpListener(IPAddress.Any, Port);
        _connectionPool = connectionPool;
    }
    
    /// <summary>
    /// Starts the TCP server.
    /// </summary>
    /// <returns></returns>
    public Task StartAsync()
    {
        _isRunning = true;
        _tcpListener.Start();
        _logger.LogInformation("Listening on {LocalEndpoint}", _tcpListener.LocalEndpoint);

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
                var tcpClientConnection = new TcpClientConnection(client, DateTime.UtcNow);
                
                _logger.LogDebug("Accepted connection from {RemoteEndpoint}", client.Client.RemoteEndPoint);
                _connectionPool.AddConnection(tcpClientConnection);
                
                ThreadPool.QueueUserWorkItem(HandleRequest, tcpClientConnection);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                _logger.LogWarning("Socket interrupted");
            }
        }
        
        _logger.LogInformation("Server stopped");
    }

    /// <summary>
    /// Handles a TCP request and forwards the request to the HTTP server.
    /// </summary>
    /// <param name="state">The <see cref="TcpClient"/> state object passed to the handler by the <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object?)"/> function.</param>
    private void HandleRequest(object? state)
    {
        if (state is not TcpClientConnection connection)
        {
            _logger.LogError("Invalid state object passed to HandleRequest: {State}", state);
            Debug.Fail("Invalid state object passed to HandleRequest");
            return;
        }
        
        try
        {
            using var stream = connection.Client.GetStream();
            var response = _requestHandler(stream);
            Debug.Assert(response.Length > 0);

            _logger.LogDebug("Sending response: Writing {ResponseBytes} bytes to buffer", response.Length);
            stream.Write(response);
            _connectionPool.CloseConnection(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An uncaught exception occurred in the request worker thread: {Message}", ex.Message);
            
            // Due to the way exceptions are handled in background threads, if a test fails due to an exception
            // being thrown then it will treat that test and every other test qs being inconclusive.
            // Only failing if the debugger is attached means that the tests will fail properly, and that
            // proper error message can be found by debugging a failing test.SSS
            if (Debugger.IsAttached)
            {
                //Debug.Fail($"{ex.GetType().Name}: Exception thrown by the TCP request handler.", ex.Message);
                throw;
            }
        }
    }
}