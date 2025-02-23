using HttpServer.Request.Parser;

namespace Tests.UnitTests;

public class StringTokenizerTests
{
    [Theory]
    [InlineData("GET / HTTP/1.1", new[] { ' ' }, new[] { "GET", "/", "HTTP/1.1" })]
    [InlineData("Host: localhost", new[] { ':' }, new[] { "Host", " localhost" })]
    [InlineData("User-Agent: xUnit", new[] { '-', ' ' }, new[] { "User", "Agent:", "xUnit" })]
    public void Tokenize_WithDifferentDelimiters_TokensAreCorrect(string input, char[] delimiters, string[] expectedTokens)
    {
        // Arrange
        var tokenizer = new StringTokenizer(input.AsSpan(), delimiters);

        // Act & Assert
        for (var i = 0; i < expectedTokens.Length; i++)
        {
            Assert.Equal(expectedTokens[i], tokenizer[i].ToString());
        }
    }

    [Fact]
    public void Tokenize_SingleDelimiter_ReturnsCorrectTokens()
    {
        // Arrange
        const string input = "GET / HTTP/1.1";
        var tokenizer = new StringTokenizer(input.AsSpan(), [' ']);

        // Act & Assert
        Assert.Equal("GET", tokenizer[0].ToString());
        Assert.Equal("/", tokenizer[1].ToString());
        Assert.Equal("HTTP/1.1", tokenizer[2].ToString());
    }

    [Fact]
    public void Tokenize_MultipleDelimiters_ReturnsCorrectTokens()
    {
        // Arrange
        const string input = "User-Agent: xUnit";
        var tokenizer = new StringTokenizer(input.AsSpan(), ['-', ' ']);

        // Act & Assert
        Assert.Equal("User", tokenizer[0].ToString());
        Assert.Equal("Agent:", tokenizer[1].ToString());
        Assert.Equal("xUnit", tokenizer[2].ToString());
    }
    
    [Fact]
    public void GetString_WithValidIndex_ReturnsCorrectToken()
    {
        // Arrange
        const string input = "GET / HTTP/1.1";
        var tokenizer = new StringTokenizer(input.AsSpan(), [' ']);

        // Act
        var token = tokenizer.GetString(1);

        // Assert
        Assert.Equal("/", token);
    }
    
    [Fact]
    public void GetString_WithInvalidIndex_ThrowsException()
    {
        // Arrange
        const string input = "GET / HTTP/1.1";

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            var tokenizer = new StringTokenizer(input.AsSpan(), [' ']);
            return tokenizer.GetString(3);
        });
    }
    
    [Fact]
    public void GetNext_WithMultipleTokens_ReturnsCorrectTokens()
    {
        // Arrange
        const string input = "GET / HTTP/1.1";
        var tokenizer = new StringTokenizer(input.AsSpan(), [' ']);

        // Act
        var token1 = tokenizer.GetNextToken();
        var token2 = tokenizer.GetNextToken();
        var token3 = tokenizer.GetNextToken();
        var token4 = tokenizer.GetNextToken();

        // Assert
        Assert.Equal("GET", token1);
        Assert.Equal("/", token2);
        Assert.Equal("HTTP/1.1", token3);
        Assert.Null(token4);
    }
    
    [Fact]
    public void GetNext_WithSingleToken_ReturnsCorrectToken()
    {
        // Arrange
        const string input = "GET";
        var tokenizer = new StringTokenizer(input.AsSpan(), [' ']);

        // Act
        var token1 = tokenizer.GetNextToken();
        var token2 = tokenizer.GetNextToken();

        // Assert
        Assert.Equal("GET", token1);
        Assert.Null(token2);
    }
    
    [Fact]
    public void Reset_WithMultipleTokens_ResetsToBeginning()
    {
        // Arrange
        const string input = "GET / HTTP/1.1";
        var tokenizer = new StringTokenizer(input.AsSpan(), [' ']);

        // Act
        var token1 = tokenizer.GetNextToken();
        var token2 = tokenizer.GetNextToken();
        tokenizer.Reset();
        var token3 = tokenizer.GetNextToken();
        var token4 = tokenizer.GetNextToken();

        // Assert
        Assert.Equal("GET", token1);
        Assert.Equal("/", token2);
        Assert.Equal("GET", token3);
        Assert.Equal("/", token4);
    }
    
    [Fact]
    public void Reset_WithSingleToken_ResetsToBeginning()
    {
        // Arrange
        const string input = "GET";
        var tokenizer = new StringTokenizer(input.AsSpan(), [' ']);

        // Act
        var token1 = tokenizer.GetNextToken();
        tokenizer.Reset();
        var token2 = tokenizer.GetNextToken();

        // Assert
        Assert.Equal("GET", token1);
        Assert.Equal("GET", token2);
    }
}