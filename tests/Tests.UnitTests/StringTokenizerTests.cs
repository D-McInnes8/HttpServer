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
}