namespace HttpServer.Networking;

/// <summary>
/// Represents a network stream reader used to parse data from a network stream.
/// </summary>
public interface INetworkStreamReader
{
    /// <summary>
    /// Reads a line from the stream.
    /// </summary>
    /// <returns>The read line as a string.</returns>
    public Task<string?> ReadLineAsync();
    
    /// <summary>
    /// Reads a specified number of bytes from the stream.
    /// </summary>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>The read bytes as a string.</returns>
    Task<string?> ReadAsync(int count);
}