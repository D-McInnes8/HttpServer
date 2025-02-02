namespace HttpServer.Headers;

/// <summary>
/// Represents the keep-alive settings for an HTTP connection.
/// </summary>
public enum HttpConnectionType
{
    /// <summary>
    /// The connection should be kept alive.
    /// </summary>
    KeepAlive,
    
    /// <summary>
    /// The connection should be closed.
    /// </summary>
    Close
}

/// <summary>
/// Represents the keep-alive settings for an HTTP connection.
/// </summary>
public class HttpKeepAlive : IEquatable<HttpKeepAlive>
{
    /// <summary>
    /// The connection type for the keep-alive connection.
    /// </summary>
    public required HttpConnectionType Connection { get; set; }
    
    /// <summary>
    /// The timeout for the keep-alive connection.
    /// </summary>
    public required TimeSpan Timeout { get; set; }
    
    /// <summary>
    /// The maximum number of requests that can be made over the keep-alive connection.
    /// </summary>
    public int? MaxRequests { get; set; }

    /// <inheritdoc />
    public bool Equals(HttpKeepAlive? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Connection == other.Connection && Timeout.Equals(other.Timeout) && MaxRequests == other.MaxRequests;
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((HttpKeepAlive)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Connection, Timeout, MaxRequests);
    }
}