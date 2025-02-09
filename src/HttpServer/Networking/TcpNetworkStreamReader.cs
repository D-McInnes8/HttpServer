using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace HttpServer.Networking;

/// <summary>
/// A network stream reader for reading data from a <see cref="TcpClientConnection"/>.
/// </summary>
public class TcpNetworkStreamReader : IDisposable
{
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private bool _bufferInitialised;
    private bool _ignoreNextNewLineCharacter;
    private bool _isDisposed;
    
    private readonly byte[] _buffer;
    private readonly int _bufferSize;
    private int _bufferPosition;
    private int _bufferLength;
    
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();
    
    /// <summary>
    /// Initializes a new instance of <see cref="TcpNetworkStreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The encoding to use when reading from the stream.</param>
    /// <param name="bufferSize">The size of the buffer to use when reading from the stream.</param>
    private TcpNetworkStreamReader(Stream stream, Encoding encoding, int bufferSize)
    {
        _stream = stream;
        _encoding = encoding;
        
        _bufferSize = bufferSize;
        _buffer = BufferPool.Rent(_bufferSize);
        _bufferPosition = 0;
        _bufferLength = 0;
    }
    
    /// <summary>
    /// Initializes a new instance of <see cref="TcpNetworkStreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="bufferSize">The size of the buffer to use when reading from the stream.</param>
    public TcpNetworkStreamReader(Stream stream, int bufferSize = 1024) : this(stream, Encoding.Default, bufferSize)
    {
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
            var span = _buffer.AsSpan(_bufferPosition, _bufferLength - _bufferPosition);
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

                // If the buffer position is at the end of the buffer and the last character was a carriage return,
                // we need to ignore the next newline character at the start of the next buffer.
                if (_bufferPosition == _bufferLength
                    && _buffer[_bufferPosition - 1] == '\r')
                {
                    _ignoreNextNewLineCharacter = true;
                }

                lineBuffer.Append(_encoding.GetString(span[..newLineIndex]));
                break;
            }

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
            var span = _buffer.AsSpan(_bufferPosition, _bufferLength - _bufferPosition);
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
        Debug.Assert(buffer.Length == count);
        return buffer.ToString();
    }
    
    /// <summary>
    /// Initialises the buffer by filling it with data from the stream.
    /// </summary>
    /// <returns></returns>
    private Task InitialiseBufferAsync()
    {
        if (_bufferInitialised && _bufferPosition < _bufferLength)
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
        _bufferLength = await _stream.ReadAsync(_buffer, 0, _bufferSize);
        _bufferPosition = 0;
        
        if (_ignoreNextNewLineCharacter)
        {
            _ignoreNextNewLineCharacter = false;
            if (_bufferLength > 0 && _buffer[0] == '\n')
            {
                _bufferPosition++;
            }
        }
        
        return _bufferLength;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _isDisposed = true;
        BufferPool.Return(_buffer, clearArray: false);
    }
}