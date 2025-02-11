using HttpServer.Networking;

namespace Tests.UnitTests;

public class TcpNetworkStreamReaderTests
{
    [Fact]
    public async Task ReadAsync_SingleCall_ShouldReadSpecifiedNumberOfBytes()
    {
        // Arrange
        var stream = CreateStream("Hello, World!");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadAsync(5);
        
        // Assert
        Assert.Equal("Hello", actual);
    }
    
    [Fact]
    public async Task ReadAsync_MultipleCalls_ShouldReadSpecifiedNumberOfBytes()
    {
        // Arrange
        var stream = CreateStream("Hello, World!");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        _ = await reader.ReadAsync(5);
        var actual = await reader.ReadAsync(7);
        
        // Assert
        Assert.Equal(", World", actual);
    }
    
    [Fact]
    public async Task ReadBytesAsync_SingleCall_ShouldReadSpecifiedNumberOfBytes()
    {
        // Arrange
        var stream = CreateStream("Hello, World!");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadBytesAsync(5);
        
        // Assert
        Assert.Equal("Hello"u8.ToArray(), actual);
    }
    
    [Fact]
    public async Task ReadBytesAsync_MultipleCalls_ShouldReadSpecifiedNumberOfBytes()
    {
        // Arrange
        var stream = CreateStream("Hello, World!");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        _ = await reader.ReadBytesAsync(5);
        var actual = await reader.ReadBytesAsync(7);
        
        // Assert
        Assert.Equal(", World"u8.ToArray(), actual);
    }
    
    [Fact]
    public async Task ReadLineAsync_SingleLine_ShouldReadLine()
    {
        // Arrange
        var stream = CreateStream("Hello, World!\r\n");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal("Hello, World!", actual);
    }
    
    [Fact]
    public async Task ReadLineAsync_EmptyLine_ShouldReturnEmptyString()
    {
        // Arrange
        var stream = CreateStream("\r\n");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal(string.Empty, actual);
    }
    
    [Fact]
    public async Task ReadLineAsync_EmptyStream_ShouldReturnEmptyString()
    {
        // Arrange
        var stream = CreateStream(string.Empty);
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadLineAsync();
        
        // Assert
        Assert.NotNull(actual);
        Assert.Empty(actual);
    }
    
    [Fact]
    public async Task ReadLineAsync_LineWithCRLF_ShouldReadLine()
    {
        // Arrange
        var stream = CreateStream("Hello, World!\r\n");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal("Hello, World!", actual);
    }
    
    [Fact]
    public async Task ReadLineAsync_LineWithLF_ShouldReadLine()
    {
        // Arrange
        var stream = CreateStream("Hello, World!\n");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal("Hello, World!", actual);
    }
    
    [Fact]
    public async Task ReadLineAsync_NoNewLineCharacter_ShouldReadLine()
    {
        // Arrange
        var stream = CreateStream("Hello, World!");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal("Hello, World!", actual);
    }
    
    [Fact]
    public async Task ReadLineAsync_MultipleLines_ShouldReadLines()
    {
        // Arrange
        var stream = CreateStream("Hello, World!\r\nHow are you?\r\n");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual1 = await reader.ReadLineAsync();
        var actual2 = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal("Hello, World!", actual1);
        Assert.Equal("How are you?", actual2);
    }
    
    [Fact]
    public async Task ReadLineAsync_LineSpansMultipleBuffers_ShouldReadLines()
    {
        // Arrange
        var stream = CreateStream("Hello, World!\r\nHow are you?\r\n");
        using var reader = new TcpNetworkStreamReader(stream, 5);
        
        // Act
        var actual1 = await reader.ReadLineAsync();
        var actual2 = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal("Hello, World!", actual1);
        Assert.Equal("How are you?", actual2);
    }

    [Fact]
    public async Task ReadLineAsync_NewLineCharacterSpansMultipleBuffers_ShouldReadLines()
    {
        // Arrange
        var stream = CreateStream("Hello, World!\r\nHow are you?\r\n");
        using var reader = new TcpNetworkStreamReader(stream, 14);
        
        // Act
        var actual1 = await reader.ReadLineAsync();
        var actual2 = await reader.ReadLineAsync();
        
        // Assert
        Assert.Equal("Hello, World!", actual1);
        Assert.Equal("How are you?", actual2);
    }
    
    [Fact]
    public async Task ReadLineAsync_ThenReadAsync_ShouldReadLineAndRemainingBytes()
    {
        // Arrange
        var stream = CreateStream("Hello, World!\r\nHow are you?");
        using var reader = new TcpNetworkStreamReader(stream);
        
        // Act
        var actual1 = await reader.ReadLineAsync();
        var actual2 = await reader.ReadAsync(12);
        
        // Assert
        Assert.Equal("Hello, World!", actual1);
        Assert.Equal("How are you?", actual2);
    }
    
    private static MemoryStream CreateStream(string content)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}