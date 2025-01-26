using System.Net;
using System.Text;
using HttpServer;
using HttpServer.Response;
using HttpServer.Response.Body;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseBodyStringTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9801).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        _httpClient.BaseAddress = new Uri($"http://localhost:{_server.Port}");
        await _server.StartAsync();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _server.StopAsync();
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
    [InlineData("ascii", "Hello, 世界!")]
    [InlineData("ascii", "こんにちは世界")]
    [InlineData("ascii", "Привет, мир!")]
    [InlineData("ascii", "Hola, mundo!")]
    [InlineData("ascii", "Bonjour, le monde!")]
    [InlineData("ascii", "Hallo, Welt!")]
    [InlineData("ascii", "안녕하세요 세계")]
    [InlineData("ascii", "مرحبا بالعالم")]
    [InlineData("ascii", "שלום עולם")]
    public async Task HttpResponseBodyString_DifferentEncodings_ShouldCorrectlyEncodeBody(string encodingName, string expectedContent)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        _server.MapGet("/api/test-encoding", _ =>
        {
            return HttpResponse.Ok(new StringBodyContent(expectedContent, encoding));
        });

        // Act
        var response = await _httpClient.GetAsync("/api/test-encoding");
        var actualBytes = await response.Content.ReadAsByteArrayAsync();
        var actualContent = encoding.GetString(actualBytes);
        var expectedBytes = encoding.GetBytes(expectedContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedBytes, actualBytes);
        Assert.Equal(expectedContent, actualContent);
    }
    
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("ascii")]
    public async Task HttpResponseBodyString_DifferentEncodings_ShouldCorrectlySetCharset(string encodingName)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        _server.MapGet("/api/test-encoding", _ =>
        {
            return HttpResponse.Ok(new StringBodyContent("Hello, World!", encoding));
        });

        // Act
        var response = await _httpClient.GetAsync("/api/test-encoding");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(encoding.WebName, response.Content.Headers.ContentType?.CharSet);
    }
}