using System.Text;
using System.Text.Json;
using HttpServer;
using HttpServer.Body;
using HttpServer.Headers;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseBodyJsonTests : IAsyncLifetime
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
    public async Task HttpResponseBodyJson_JsonContent_ShouldSetBody()
    {
        // Arrange
        var json = new { Message = "Hello, World!" };
        var expected = JsonSerializer.Serialize(json);
        _server.MapGet("/test", _ => HttpResponse.Json(HttpResponseStatusCode.OK, json));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task HttpResponseBodyJson_EmptyJsonContent_ShouldReturnEmptyObject()
    {
        // Arrange
        var json = new { };
        var expected = JsonSerializer.Serialize(json);
        _server.MapGet("/test", _ => HttpResponse.Json(HttpResponseStatusCode.OK, json));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task HttpResponseBodyJson_EmptyJsonArray_ShouldReturnEmptyArray()
    {
        // Arrange
        var json = Array.Empty<object>();
        var expected = JsonSerializer.Serialize(json);
        _server.MapGet("/test", _ => HttpResponse.Json(HttpResponseStatusCode.OK, json));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task HttpResponseBodyJson_NullJsonContent_ShouldReturnNull()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Json<object>(HttpResponseStatusCode.OK, null!));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal("null", actual);
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
    public async Task HttpResponseBodyJson_EncodedJsonContent_ShouldSetBody(string encodingName, string expectedContent)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        var json = new { Message = expectedContent };
        var expected = JsonSerializer.Serialize(json);
        _server.MapGet("/test", _ => new HttpResponse(
            statusCode: HttpResponseStatusCode.OK,
            body: new JsonBodyContent<dynamic>(json, HttpContentType.ApplicationJson, encoding)));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }
    
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("ascii")]
    public async Task HttpResponseBodyJson_Encoding_ShouldSetContentTypeHeader(string encodingName)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        var json = new { Message = "Hello, World!" };
        _server.MapGet("/test", _ => new HttpResponse(
            statusCode: HttpResponseStatusCode.OK,
            body: new JsonBodyContent<dynamic>(json, HttpContentType.ApplicationJson, encoding)));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
        Assert.Equal(encoding.WebName, response.Content.Headers.ContentType.CharSet);
    }
    
    [Fact]
    public async Task HttpResponseBodyJson_ShouldSetContentLengthHeader()
    {
        // Arrange
        var json = new { Message = "Hello, World!" };
        var expected = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(json)).Length;
        _server.MapGet("/test", _ => HttpResponse.Json(HttpResponseStatusCode.OK, json));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = response.Content.Headers.ContentLength;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}