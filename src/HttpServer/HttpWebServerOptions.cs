namespace HttpServer;

/// <summary>
/// Used to configure options for the web server.
/// </summary>
public class HttpWebServerOptions
{
    /// <summary>
    /// Options for configuring keep-alive behavior.
    /// </summary>
    public HttpWebServerKeepAliveOptions KeepAlive { get; set; } = new HttpWebServerKeepAliveOptions();
    
    /// <summary>
    /// The maximum number of connections to allow on the server.
    /// </summary>
    public int MaxConnections { get; set; } = 100;
}

public class HttpWebServerKeepAliveOptions
{
    /// <summary>
    /// The amount of time to wait for the next request before closing the connection.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    
    /// <summary>
    /// The maximum number of requests to allow on a single connection.
    /// </summary>
    public int? MaxRequests { get; set; }
}