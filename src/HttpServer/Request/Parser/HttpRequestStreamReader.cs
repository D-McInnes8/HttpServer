using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace HttpServer.Request.Parser;

/// <summary>
/// Reader for parsing an HTTP request from a stream. Similar to <see cref="StreamReader"/>, but optimized for HTTP requests.
/// </summary>
public class HttpRequestStreamReader : IDisposable
{
    private readonly Stream _stream;
    private int _streamPosition;
    
    private readonly byte[] _buffer;
    private int _bufferPosition;
    private int _bufferLength;
    private readonly Encoding _encoding;
    private string LineBuffer => _encoding.GetString(_buffer.AsSpan(0, _bufferLength));

    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();
    //private const int BufferSize = 4096;
    private const int BufferSize = 2048;
    
    /// <summary>
    /// Creates a new <see cref="HttpRequestStreamReader"/> with the specified stream.
    /// </summary>
    /// <param name="stream">The request stream to be processed.</param>
    public HttpRequestStreamReader(Stream stream)
    {
        _stream = stream;
        _buffer = BufferPool.Rent(BufferSize);
        _bufferPosition = 0;
        _bufferLength = 0;
        _encoding = Encoding.UTF8;
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        BufferPool.Return(_buffer, clearArray: true);
    }
    
    /// <summary>
    /// Reads a line from the stream.
    /// </summary>
    /// <returns>The read line as a string.</returns>
    public async Task<string?> ReadLineAsync()
    {
        if (_streamPosition == 0)
        {
            await FillBufferAsync();
        }
        
        // If the buffer is empty, we have reached the end of the stream.
        if (_bufferLength == 0)
        {
            return null;
        }
        
        var stringBuilder = new StringBuilder();
        do
        {
            ReadOnlySpan<byte> bufferSpan = _buffer.AsSpan(_bufferPosition);
            Debug.Assert(bufferSpan.Length > 0);
            
            var newLineIndex = bufferSpan.IndexOfAny((byte) '\r', (byte) '\n');
            if (newLineIndex >= 0)
            {
                var segment = _encoding.GetString(bufferSpan[..newLineIndex]);
                _bufferPosition += newLineIndex + 1;
                
                // TODO: Handle CRLF line endings if the character spans multiple buffers.
                if (newLineIndex + 1 < bufferSpan.Length
                    && bufferSpan[newLineIndex] == '\r' && bufferSpan[newLineIndex + 1] == '\n')
                {
                    _bufferPosition++;
                }
                
                stringBuilder.Append(segment);
                return stringBuilder.ToString();
            }
            
            stringBuilder.Append(_encoding.GetString(bufferSpan));
        } while (await FillBufferAsync() > 0);
        
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Reads the remaining content of the stream.
    /// </summary>
    /// <param name="contentLength">The content length of the body as specified by the content length header.</param>
    /// <returns>The remaining request content as a string.</returns>
    public Task<string?> ReadToEndAsync(int contentLength)
    {
        if (_bufferLength == 0)
        {
            return Task.FromResult<string?>(null);
        }
        
        var buffer = ArrayPool<char>.Shared.Rent(contentLength);
        var bufferSpan = buffer.AsSpan().Slice(0, contentLength);
        var bufferPos = 0;

        try
        {
            do
            {
                var numBytesToRead = _bufferLength - _bufferPosition;
                var writeBuffer = bufferSpan.Slice(bufferPos, numBytesToRead);
                var readBuffer = _buffer.AsSpan(_bufferPosition, numBytesToRead);
                var actualBytesRead = _encoding.GetChars(readBuffer, writeBuffer);

                Debug.Assert(actualBytesRead == numBytesToRead);
                bufferPos += actualBytesRead;
            } while (FillBuffer() > 0);

            return Task.FromResult<string?>(new string(bufferSpan));
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Fills the buffer with data from the stream.
    /// </summary>
    /// <returns>The number of bytes read from the stream.</returns>
    private int FillBuffer()
    {
        // If the buffer is currently not full, we have reached the end of the stream.
        if (_bufferLength != 0 && _bufferLength < BufferSize)
        {
            return 0;
        }
        
        _bufferLength = _stream.Read(_buffer, 0, BufferSize);
        _bufferPosition = 0;
        _streamPosition += _bufferLength;
        return _bufferLength;
    }
    
    /// <summary>
    /// Fills the buffer with data from the stream asynchronously.
    /// </summary>
    /// <returns>The number of bytes read from the stream.</returns>
    private async Task<int> FillBufferAsync()
    {
        // If the buffer is currently not full, we have reached the end of the stream.
        if (_bufferLength != 0 && _bufferLength < BufferSize)
        {
            return 0;
        }
        
        _bufferLength = await _stream.ReadAsync(_buffer.AsMemory(0, BufferSize));
        _bufferPosition = 0;
        _streamPosition += _bufferLength;
        return _bufferLength;
    }
}