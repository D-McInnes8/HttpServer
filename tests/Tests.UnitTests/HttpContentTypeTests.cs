using Application;

namespace Tests.UnitTests;

public class HttpContentTypeTests
{
    [Theory]
    [InlineData("text/plain", "text")]
    [InlineData("application/json", "application")]
    [InlineData("image/jpeg", "image")]
    [InlineData("audio/mpeg", "audio")]
    [InlineData("video/mp4", "video")]
    [InlineData("application/pdf", "application")]
    [InlineData("application/xml", "application")]
    [InlineData("application/zip", "application")]
    [InlineData("application/x-www-form-urlencoded", "application")]
    [InlineData("multipart/form-data", "multipart")]
    [InlineData("message/http", "message")]
    [InlineData("message/partial", "message")]
    [InlineData("message/rfc822", "message")]
    [InlineData("multipart/alternative", "multipart")]
    [InlineData("multipart/digest", "multipart")]
    [InlineData("multipart/encrypted", "multipart")]
    [InlineData("multipart/mixed", "multipart")]
    [InlineData("multipart/parallel", "multipart")]
    [InlineData("multipart/related", "multipart")]
    public void HttpContentType_ParseString_ShouldSetType(string contentType, string expected)
    {
        // Act
        var actual = HttpContentType.Parse(contentType);
        
        // Assert
        Assert.Equal(expected, actual.Type);
    }
    
    [Theory]
    [InlineData("text/plain", "plain")]
    [InlineData("application/json", "json")]
    [InlineData("image/jpeg", "jpeg")]
    [InlineData("audio/mpeg", "mpeg")]
    [InlineData("video/mp4", "mp4")]
    [InlineData("application/pdf", "pdf")]
    [InlineData("application/xml", "xml")]
    [InlineData("application/zip", "zip")]
    [InlineData("application/x-www-form-urlencoded", "x-www-form-urlencoded")]
    [InlineData("multipart/form-data", "form-data")]
    [InlineData("message/http", "http")]
    [InlineData("message/partial", "partial")]
    [InlineData("message/rfc822", "rfc822")]
    [InlineData("multipart/alternative", "alternative")]
    [InlineData("multipart/digest", "digest")]
    [InlineData("multipart/encrypted", "encrypted")]
    [InlineData("multipart/mixed", "mixed")]
    [InlineData("multipart/parallel", "parallel")]
    [InlineData("multipart/related", "related")]
    public void HttpContentType_ParseString_ShouldSetSubType(string contentType, string expected)
    {
        // Act
        var actual = HttpContentType.Parse(contentType);
        
        // Assert
        Assert.Equal(expected, actual.SubType);
    }
    
    [Theory]
    [InlineData("text/plain; charset=utf-8", "utf-8")]
    [InlineData("text/plain; charset=iso-8859-1", "iso-8859-1")]
    [InlineData("text/plain; charset=windows-1252", "windows-1252")]
    [InlineData("text/plain; charset=us-ascii", "us-ascii")]
    [InlineData("text/plain; charset=iso-2022-jp", "iso-2022-jp")]
    [InlineData("text/plain; charset=shift_jis", "shift_jis")]
    [InlineData("text/plain; charset=euc-jp", "euc-jp")]
    [InlineData("text/plain; charset=big5", "big5")]
    [InlineData("text/plain; charset=gb2312", "gb2312")]
    [InlineData("text/plain; charset=iso-8859-2", "iso-8859-2")]
    [InlineData("text/plain; charset=ascii", "ascii")]
    [InlineData("text/plain; charset=ansi", "ansi")]
    public void HttpContentType_ParseString_ShouldSetCharset(string contentType, string expected)
    {
        // Act
        var actual = HttpContentType.Parse(contentType);
        
        // Assert
        Assert.Equal(expected, actual.Charset);
    }
    
    [Theory]
    [InlineData("text/plain; boundary=boundary", "boundary")]
    public void HttpContentType_ParseString_ShouldSetBoundary(string contentType, string expected)
    {
        // Act
        var actual = HttpContentType.Parse(contentType);
        
        // Assert
        Assert.Equal(expected, actual.Boundary);
    }

    [Theory]
    [InlineData("text/plain; charset=utf-8", "charset", "utf-8")]
    [InlineData("text/plain; param=value", "param", "value")]
    [InlineData("text/plain; param=value; param2=value2", "param", "value")]
    public void HttpContentType_ParseString_ShouldSetCustomParameters(string contentType, string key, string expected)
    {
        // Act
        var actual = HttpContentType.Parse(contentType);
        
        // Assert
        Assert.Equal(expected, actual.Parameters[key]);
    }
    
    [Fact]
    public void HttpContentType_TryParseValidString_ShouldReturnTrue()
    {
        // Arrange
        const string contentType = "text/plain";
        
        // Act
        var actual = HttpContentType.TryParse(contentType, out _);
        
        // Assert
        Assert.True(actual);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("text")]
    public void HttpContentType_TryParseInvalidString_ShouldReturnFalse(string contentType)
    {
        // Act
        var actual = HttpContentType.TryParse(contentType, out _);
        
        // Assert
        Assert.False(actual);
    }
    
    [Theory]
    [InlineData("text", "plain")]
    [InlineData("application", "json")]
    [InlineData("image", "jpeg")]
    [InlineData("audio", "mpeg")]
    [InlineData("video", "mp4")]
    public void HttpContentType_Constructor_ShouldSetType(string type, string subType)
    {
        // Act
        var actual = new HttpContentType(type, subType);
        
        // Assert
        Assert.Equal(type, actual.Type);
    }
    
    [Theory]
    [InlineData("text", "plain")]
    [InlineData("application", "json")]
    [InlineData("image", "jpeg")]
    [InlineData("audio", "mpeg")]
    [InlineData("video", "mp4")]
    public void HttpContentType_Constructor_ShouldSetSubType(string type, string subType)
    {
        // Act
        var actual = new HttpContentType(type, subType);
        
        // Assert
        Assert.Equal(subType, actual.SubType);
    }
}