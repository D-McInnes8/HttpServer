namespace HttpServer.Networking;

/// <summary>
/// Represents a connection pool for TCP connections.
/// </summary>
public interface IConnectionPool
{
    /// <summary>
    /// Adds a connection to the pool.
    /// </summary>
    /// <param name="connection">The connection to add to the pool.</param>
    public void AddConnection(TcpClientConnection connection);
    
    /// <summary>
    /// Closes the connection and removes it from the pool.
    /// </summary>
    /// <param name="connection">The connection to close.</param>
    public void CloseConnection(TcpClientConnection connection);
}

/// <summary>
/// Represents a connection pool for TCP connections.
/// </summary>
public class TcpConnectionPool : IConnectionPool, IAsyncDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly List<TcpClientConnection> _connections;
    private readonly HttpWebServerOptions _options;
    private readonly Timer _expiredConnectionTimer;
    private readonly Lock _lock;

    /// <summary>
    /// Initializes a new instance of <see cref="TcpConnectionPool"/>.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for the connection pool.</param>
    /// <param name="options">The options for the web server.</param>
    public TcpConnectionPool(TimeProvider timeProvider, HttpWebServerOptions options)
    {
        _timeProvider = timeProvider;
        _options = options;
        _connections = new List<TcpClientConnection>(capacity: options.MaxConnections);
        _expiredConnectionTimer = new Timer(RemoveExpiredConnections, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        _lock = new Lock();
    }

    /// <inheritdoc />
    public void AddConnection(TcpClientConnection connection)
    {
        lock (_lock)
        {
            _connections.Add(connection);
        }
    }

    /// <inheritdoc />
    public void CloseConnection(TcpClientConnection connection)
    {
        lock (_lock)
        {
            _connections.Remove(connection);
            connection.Client.Close();
        }
    }

    /// <summary>
    /// Removes connections that have expired.
    /// </summary>
    private void RemoveExpiredConnections(object? _)
    {
        lock (_lock)
        {
            var invocationDateTime = _timeProvider.GetUtcNow();
            foreach (var connection in _connections)
            {
                if (connection.ConnectionOpened.Add(_options.KeepAlive.Timeout) < invocationDateTime)
                {
                    connection.Client.Close();
                }
            }
            _connections.RemoveAll(c => c.ConnectionOpened.Add(_options.KeepAlive.Timeout) < invocationDateTime);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _expiredConnectionTimer.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}