using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace HttpServer.Networking;

/// <summary>
/// A network stream reader for reading data from a <see cref="TcpClientConnection"/>.
/// </summary>
internal class TcpNetworkStreamReader : IDisposable
{
    private readonly TcpClientConnection _tcpClientConnection;
    private readonly Encoding _encoding;
    private bool _bufferInitialised;
    private bool _isDisposed;
    
    private readonly byte[] _buffer;
    private int _bufferPosition;
    private int _bufferLength;
    
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();
    private const int BufferSize = 2048;

    /// <summary>
    /// Initializes a new instance of <see cref="TcpNetworkStreamReader"/>.
    /// </summary>
    /// <param name="tcpClientConnection"></param>
    internal TcpNetworkStreamReader(TcpClientConnection tcpClientConnection)
    {
        _tcpClientConnection = tcpClientConnection;
        _encoding = Encoding.ASCII;
        
        _buffer = BufferPool.Rent(BufferSize);
        _bufferPosition = 0;
        _bufferLength = 0;
    }

    /// <summary>
    /// Reads a line from the stream.
    /// </summary>
    /// <returns>The read line as a string.</returns>
    public async Task<string?> ReadLineAsync()
    {
        if (_isDisposed)
        {
            return null;
        }
        
        await InitialiseBufferAsync();
        var lineBuffer = new StringBuilder();
        do
        {
            var span = _buffer.AsSpan(_bufferPosition);
            var newLineIndex = span.IndexOfAny((byte)'\r', (byte)'\n');

            if (newLineIndex != -1)
            {
                _bufferPosition += newLineIndex + 1;

                // Skip the next character if it is a newline character and the previous character was a carriage return.
                if (_bufferPosition < _bufferLength
                    && _buffer[_bufferPosition - 1] == '\r'
                    && _buffer[_bufferPosition] == '\n')
                {
                    _bufferPosition++;
                }

                lineBuffer.Append(_encoding.GetString(span[..newLineIndex]));
                break;
            }

            //_bufferPosition = _bufferLength;
            lineBuffer.Append(_encoding.GetString(span));
        } while (await FillBufferAsync() > 0);
        
        return lineBuffer.ToString();
    }

    /// <summary>
    /// Reads a specified number of bytes from the stream.
    /// </summary>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>The read bytes as a string.</returns>
    public async Task<string?> ReadAsync(int count)
    {
        if (_isDisposed || count <= 0)
        {
            return null;
        }
        
        await InitialiseBufferAsync();
        var buffer = new StringBuilder();
        var remaining = count;
        do
        {
            var span = _buffer.AsSpan(_bufferPosition);
            var bytesToRead = Math.Min(remaining, span.Length);

            buffer.Append(_encoding.GetString(span[..bytesToRead]));
            _bufferPosition += bytesToRead;

            remaining -= bytesToRead;
            if (remaining == 0)
            {
                break;
            }
        } while (await FillBufferAsync() > 0);

        Debug.Assert(remaining == 0);
        return buffer.ToString();
    }
    
    /// <summary>
    /// Initialises the buffer by filling it with data from the stream.
    /// </summary>
    /// <returns></returns>
    private Task InitialiseBufferAsync()
    {
        if (_bufferInitialised)
        {
            return Task.CompletedTask;
        }

        _bufferInitialised = true;
        return FillBufferAsync();
    }
    
    /// <summary>
    /// Fills the buffer with data from the stream.
    /// </summary>
    /// <returns>The number of bytes read from the stream.</returns>
    private async Task<int> FillBufferAsync()
    {
        _bufferLength = await _tcpClientConnection.Stream.ReadAsync(_buffer, 0, BufferSize);
        _bufferPosition = 0;
        return _bufferLength;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _isDisposed = true;
        BufferPool.Return(_buffer, clearArray: false);
    }
}