using System.Buffers;

namespace HttpServer.Response.Internal;

/// <summary>
/// 
/// </summary>
public class ResponseBuffer : IDisposable
{
    private byte[] _buffer;
    private int _position;

    /// <summary>
    /// 
    /// </summary>
    public ResponseBuffer() : this(1024)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bufferSize"></param>
    public ResponseBuffer(int bufferSize)
    {
        _buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        _position = 0;
    }

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

    public void CopyTo(Span<byte> destination)
    {
        _buffer.AsSpan(0, _position).CopyTo(destination);
    }

    public Span<byte> AsSpan(int length)
    {
        CheckLengthAndReallocate(length);
        var span = _buffer.AsSpan(_position, length);
        _position += length;
        return span;
    }

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