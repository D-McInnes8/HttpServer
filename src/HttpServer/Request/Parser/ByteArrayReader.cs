namespace HttpServer.Request.Parser;

/// <summary>
/// Represents a reader for a byte array.
/// </summary>
public ref struct ByteArrayReader
{
    private readonly ReadOnlySpan<byte> _byteArray;
    private int _position;
    
    /// <summary>
    /// Constructs a new <see cref="ByteArrayReader"/> with the specified byteArray.
    /// </summary>
    /// <param name="byteArray">The byteArray to read.</param>
    public ByteArrayReader(ReadOnlySpan<byte> byteArray)
    {
        _byteArray = byteArray;
        _position = 0;
    }
    
    public ReadOnlySpan<byte> ReadUntilBytes(byte delimiter)
    {
        var index = _byteArray[_position..].IndexOf(delimiter);
        if (index == -1)
        {
            return _byteArray[_position..];
        }

        var result = _byteArray.Slice(_position, index);
        _position += index + 1;
        return result;
    }

    public ReadOnlySpan<byte> ReadLineBytes()
    {
        var newLineIndex = _byteArray[_position..].IndexOfAny((byte)'\r', (byte)'\n');
        if (newLineIndex == -1)
        {
            return _byteArray[_position..];
        }
        
        var result = _byteArray.Slice(_position, newLineIndex);
        _position += newLineIndex + 1;
        
        if (_position < _byteArray.Length
            && _byteArray[_position - 1] == '\r'
            && _byteArray[_position] == '\n')
        {
            _position++;
        }
        
        return result;
    }

    public ReadOnlySpan<byte> ReadToEndBytes()
    {
        var result = _byteArray[_position..];
        _position += result.Length;
        return result;
    }
}