using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Application;

public class TcpServer
{
    public int Port { get; private set; }
    public Uri LocalEndpoint => new Uri(_tcpListener.LocalEndpoint.ToString()!);
    
    private readonly TcpListener _tcpListener;
    private bool _isRunning;
    
    private readonly Func<string, string> _requestHandler;

    public TcpServer(int port, Func<string, string> requestHandler)
    {
        Port = port;
        _requestHandler = requestHandler;
        _tcpListener = new TcpListener(IPAddress.Any, Port);
    }
    public Task StartAsync()
    {
        _isRunning = true;
        _tcpListener.Start();
        Console.WriteLine($"Listening on port {_tcpListener.LocalEndpoint}");

        var thread = new Thread(ListenAsync);
        thread.Start();
        
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _isRunning = false;
        _tcpListener.Stop();
        return Task.CompletedTask;
    }
    
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
        
        var response = _requestHandler(message);
                
        /*var body = "Hello World";
        var responseMessage = $"""
                               HTTP/1.1 200 OK
                               Date: {DateTime.UtcNow:R}
                               Content-Length: {body.Length}
                               Content-Type: text/plain

                               {body}
                               """;*/
        stream.Write(Encoding.UTF8.GetBytes(response));
        client.Close();
    }
}