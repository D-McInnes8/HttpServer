using System.Buffers;

namespace HttpServer.Response.Internal;

/// <summary>
/// Represents a buffer for a HTTP response.
/// </summary>
public class ResponseBuffer : IDisposable
{
    private byte[] _buffer;
    private int _position;
    
    /// <summary>
    /// The length of the buffer.
    /// </summary>
    public int Length => _position;

    /// <summary>
    /// Constructs a new <see cref="ResponseBuffer"/> with a default buffer size of 1024 bytes.
    /// </summary>
    public ResponseBuffer() : this(1024)
    {
    }

    /// <summary>
    /// Constructs a new <see cref="ResponseBuffer"/> with the specified buffer size.
    /// </summary>
    /// <param name="bufferSize"></param>
    public ResponseBuffer(int bufferSize)
    {
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        _position = 0;
    }

    /// <summary>
    /// Appends the specified data to the buffer.
    /// </summary>
    /// <param name="data">The data to append.</param>
    public void Append(ReadOnlySpan<byte> data)
    {
        /*if (_buffer.Length - _position < data.Length)
        {
            var newSize = _buffer.Length - _position + data.Length;
            var tmpBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Array.Copy(_buffer, 0, tmpBuffer, 0, _position);
            
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = tmpBuffer;
        }*/

        CheckLengthAndReallocate(data.Length);
        data.CopyTo(_buffer.AsSpan(_position, data.Length));
        _position += data.Length;
    }

    /// <summary>
    /// Copies the buffer to the specified destination.
    /// </summary>
    /// <param name="destination">The destination to copy the buffer to.</param>
    public void CopyTo(Span<byte> destination)
    {
        _buffer.AsSpan(0, _position).CopyTo(destination);
    }

    /// <summary>
    /// Returns the buffer as a span of the specified length.
    /// </summary>
    /// <param name="length">The length of the span.</param>
    /// <returns>The buffer as a span of the specified length.</returns>
    public Span<byte> AsSpan(int length)
    {
        CheckLengthAndReallocate(length);
        var span = _buffer.AsSpan(_position, length);
        _position += length;
        return span;
    }

    /// <summary>
    /// Returns the buffer as a read-only span.
    /// </summary>
    /// <returns>The buffer as a read-only span.</returns>
    public ReadOnlySpan<byte> AsReadOnlySpan()
    {
        return _buffer.AsSpan(0, _position);
    }

    private void CheckLengthAndReallocate(int length)
    {
        if (_buffer.Length - _position < length)
        {
            var newSize = _position + length;
            var tmpBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Array.Copy(_buffer, 0, tmpBuffer, 0, _position);
            
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = tmpBuffer;
        }
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_buffer, clearArray: false);
    }
}