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
    [InlineData("utf-16", "Hello, World!")]
    [InlineData("ascii", "Hello, World!")]
    public async Task HttpResponseBodyString_DifferentEncodings_ShouldCorrectlyEncodeBody(string encodingName, string expectedContent)
    {
        // Arrange
        var encoding = Encoding.GetEncoding(encodingName);
        _server.MapGet("/api/test-encoding", _ =>
        {
            return HttpResponse.Ok(new StringBodyContent("Hello, World!", encoding));
        });

        // Act
        var response = await _httpClient.GetAsync("/api/test-encoding");
        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        var responseContent = encoding.GetString(responseBytes);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedContent, responseContent);
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