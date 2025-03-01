using System.Text;
using System.Text.Json;
using HttpServer;
using HttpServer.Body;
using HttpServer.Request;
using HttpServer.Response;
using HttpServer.Routing;

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
    
    [Fact]
    public async Task HttpRequestBodyJson_EmptyJsonContent_ShouldSetBody()
    {
        // Arrange
        var json = new { };
        var expected = JsonSerializer.Serialize(json);
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        _ = await _httpClient.PostAsJsonAsync("/test", json);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<dynamic>>(request?.Body);
        Assert.NotNull(actual);
        Assert.Equal(expected, actual.Encoding.GetString(actual.Content));
    }
    
    [Fact]
    public async Task HttpRequestBodyJson_EmptyJsonArray_ShouldSetBody()
    {
        // Arrange
        var json = new byte[] { };
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        _ = await _httpClient.PostAsJsonAsync("/test", json);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<byte[]>>(request?.Body).Deserialize();
        Assert.NotNull(actual);
        Assert.Empty(actual);
    }
    
    [Fact]
    public async Task HttpRequestBodyJson_NullJsonContent_ShouldSetBody()
    {
        // Arrange
        var json = (object?)null;
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        _ = await _httpClient.PostAsJsonAsync("/test", json);
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<object?>>(request?.Body).Deserialize();
        Assert.Null(actual);
    }
    
    [Theory]
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
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        _ = await _httpClient.PostAsync(
            requestUri: "/test",
            content: new StringContent(JsonSerializer.Serialize(expected), encoding, "application/json"));
        
        // Assert
        var actual = Assert.IsType<JsonBodyContent<dynamic>>(request?.Body).Deserialize();
        Assert.NotNull(actual);
        Assert.Equivalent(expected, actual);
    }
    
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("ascii")]
    public async Task HttpRequestBodyJson_Encoding_ShouldSetContentTypeHeader(string encodingName)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        var expected = new { Message = "Hello, World!" };
        HttpRequest? request = null;
        _server.MapPost("/test", ctx =>
        {
            request = ctx.Request;
            return HttpResponse.Ok();
        });
        
        // Act
        _ = await _httpClient.PostAsync(
            requestUri: "/test",
            content: new StringContent(JsonSerializer.Serialize(expected), encoding, "application/json"));
        
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