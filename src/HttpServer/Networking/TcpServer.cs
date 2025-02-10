using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpServer.Headers;
using HttpServer.Response;
using HttpServer.Response.Internal;
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
    public int Port
    {
        get
        {
            if (_tcpListener.LocalEndpoint is IPEndPoint ipEndPoint)
            {
                return ipEndPoint.Port;
            }

            return -1;
        }
    }
    
    /// <summary>
    /// The local endpoint the server is listening on.
    /// </summary>
    public Uri LocalEndpoint => new Uri(_tcpListener.LocalEndpoint.ToString()!);
    
    private readonly TcpListener _tcpListener;
    private bool _isRunning;
    private HttpWebServerOptions _options;
    private readonly ILogger<TcpServer> _logger;

    private readonly Func<INetworkStreamReader, HttpResponse> _requestHandler;
    private readonly IConnectionPool _connectionPool;

    /// <summary>
    /// Creates a new <see cref="TcpServer"/> with the specified port and request handler.
    /// </summary>
    /// <param name="port">The port the TCP server will listen on.</param>
    /// <param name="requestHandler">The request handler to execute when receiving a TCP request.</param>
    /// <param name="logger">The <see cref="ILogger"/> object to be logged to.</param>
    /// <param name="connectionPool">The <see cref="IConnectionPool"/> object to manage connections.</param>
    /// <param name="options">The <see cref="HttpWebServerOptions"/> object containing the server options.</param>
    public TcpServer(int port, Func<INetworkStreamReader, HttpResponse> requestHandler, ILogger<TcpServer> logger, IConnectionPool connectionPool, HttpWebServerOptions options)
    {
        _requestHandler = requestHandler;
        _logger = logger;
        _options = options;
        _tcpListener = new TcpListener(IPAddress.Any, port);
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

                ThreadPool.QueueUserWorkItem(ListenToTcpSession, tcpClientConnection);
                //ThreadPool.QueueUserWorkItem(HandleRequest, tcpClientConnection);
                //ThreadPool.QueueUserWorkItem(ListenToSocket, tcpClientConnection);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                _logger.LogWarning("Socket interrupted");
            }
        }
        
        _logger.LogInformation("Server stopped");
    }

    private void ListenToTcpSession(object? state)
    {
        if (state is not TcpClientConnection connection)
        {
            _logger.LogError("Invalid state object passed to HandleRequest: {State}", state);
            Debug.Fail("Invalid state object passed to HandleRequest");
            return;
        }

        try
        {
            var stream = connection.Client.GetStream();
            using var streamReader = new TcpNetworkStreamReader(stream);
            var response = _requestHandler(streamReader);
            var buffer = HttpResponseWriter.WriteResponse(response);
            Debug.Assert(buffer.Length > 0);

            _logger.LogDebug("Sending response: Writing {ResponseBytes} bytes to buffer", buffer.Length);
            stream.Write(buffer);

            if (response.KeepAlive.Connection == HttpConnectionType.Close)
            {
                _logger.LogDebug("Closing connection to {RemoteEndpoint}", connection.Client.Client.RemoteEndPoint);
                _connectionPool.CloseConnection(connection);
            }
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
        finally
        {
            _connectionPool.CloseConnection(connection);
        }
    }

    /// <summary>
    /// Handles a TCP request and forwards the request to the HTTP server.
    /// </summary>
    /// <param name="state">The <see cref="TcpClient"/> state object passed to the handler by the <see cref="ThreadPool.QueueUserWorkItem(WaitCallback, object?)"/> function.</param>
    [Obsolete]
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
            var stream = connection.Client.GetStream();
            using var streamReader = new TcpNetworkStreamReader(stream);
            var response = _requestHandler(streamReader);
            var buffer = HttpResponseWriter.WriteResponse(response);
            Debug.Assert(buffer.Length > 0);

            _logger.LogDebug("Sending response: Writing {ResponseBytes} bytes to buffer", buffer.Length);
            stream.Write(buffer);

            if (response.KeepAlive.Connection == HttpConnectionType.Close)
            {
                _logger.LogDebug("Closing connection to {RemoteEndpoint}", connection.Client.Client.RemoteEndPoint);
                _connectionPool.CloseConnection(connection);
            }
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
        finally
        {
            _connectionPool.CloseConnection(connection);
        }
    }
}