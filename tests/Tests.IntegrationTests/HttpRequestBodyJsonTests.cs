using System.Text;
using System.Text.Json;
using HttpServer;
using HttpServer.Body;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;
using Tests.IntegrationTests.TestExtensions;

namespace Tests.IntegrationTests;

public class HttpRequestBodyJsonTests: IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(0).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        await _server.StartAsync();
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _server.StopAsync();
    }
    
    [Fact(Skip = "Not implemented yet")]
    public async Task HttpRequestBodyJson_EmptyJsonContent_ShouldSetBody()
    {
        // Arrange
        var json = new { };
        var expected = JsonSerializer.Serialize(json);
        var content = new StringContent(expected, Encoding.UTF8, "application/json");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<dynamic>>(request.Body);
        Assert.NotNull(actual);
        Assert.Equal(expected, actual.Encoding.GetString(actual.Content));
    }
    
    [Fact(Skip = "Not implemented yet")]
    public async Task HttpRequestBodyJson_EmptyJsonArray_ShouldSetBody()
    {
        // Arrange
        var json = new byte[] { };
        var content = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<byte[]>>(request?.Body).Deserialize();
        Assert.NotNull(actual);
        Assert.Empty(actual);
    }
    
    [Fact(Skip = "Not implemented yet")]
    public async Task HttpRequestBodyJson_NullJsonContent_ShouldSetBody()
    {
        // Arrange
        var json = (object?)null;
        var content = new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<object?>>(request.Body).Deserialize();
        Assert.Null(actual);
    }
    
    [Theory(Skip = "Not implemented yet")]
    [InlineData("utf-8", "Hello, World!")]
    [InlineData("utf-8", "Hello, 世界!")]
    [InlineData("utf-8", "こんにちは世界")]
    [InlineData("utf-8", "Привет, мир!")]
    [InlineData("utf-8", "Hola, mundo!")]
    [InlineData("utf-8", "Bonjour, le monde!")]
    [InlineData("utf-8", "Hallo, Welt!")]
    [InlineData("utf-8", "안녕하세요 세계")]
    [InlineData("utf-8", "مرحبا بالعالم")]
    [InlineData("utf-8", "שלום עולם")]
    [InlineData("utf-16", "Hello, World!")]
    [InlineData("utf-16", "Hello, 世界!")]
    [InlineData("utf-16", "こんにちは世界")]
    [InlineData("utf-16", "Привет, мир!")]
    [InlineData("utf-16", "Hola, mundo!")]
    [InlineData("utf-16", "Bonjour, le monde!")]
    [InlineData("utf-16", "Hallo, Welt!")]
    [InlineData("utf-16", "안녕하세요 세계")]
    [InlineData("utf-16", "مرحبا بالعالم")]
    [InlineData("utf-16", "שלום עולם")]
    [InlineData("ascii", "Hello, World!")]
    [InlineData("ascii", "Hola, mundo!")]
    [InlineData("ascii", "Bonjour, le monde!")]
    [InlineData("ascii", "Hallo, Welt!")]
    public async Task HttpRequestBodyJson_EncodedJsonContent_ShouldSetBody(string encodingName, string expectedContent)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        var expected = new { Message = expectedContent };
        var content = new StringContent(JsonSerializer.Serialize(expected), encoding, "application/json");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<dynamic>>(request.Body).Deserialize();
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
    
    [Theory(Skip = "Not implemented yet")]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("ascii")]
    public async Task HttpRequestBodyJson_Encoding_ShouldSetContentTypeHeader(string encodingName)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        var expected = new { Message = "Hello, World!" };
        var content = new StringContent(JsonSerializer.Serialize(expected), encoding, "application/json");
        
        // Act
        var request = await _server.PostAsyncAndCaptureRequest("/test", content);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<dynamic>>(request?.Body);
        Assert.NotNull(actual.ContentType);
        Assert.Multiple(() =>
        {
            Assert.Equal("application/json", actual.ContentType.MediaType);
            Assert.Equal(encoding.WebName, actual.ContentType.Charset);
            Assert.Equal(encoding.WebName, actual.Encoding.WebName);
        });
    }
}