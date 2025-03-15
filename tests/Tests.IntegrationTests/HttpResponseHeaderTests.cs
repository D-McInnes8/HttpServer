using System.Text;
using HttpServer;
using HttpServer.Body;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseHeaderTests: IAsyncLifetime
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

    [Theory]
    [InlineData("Hello, World!")]
    public async Task HttpResponseHeader_ContentLength_ShouldSetContentLengthHeader(string content)
    {
        // Arrange
        var expected = Encoding.UTF8.GetBytes(content).Length;
        _server.MapGet("/test", _ => HttpResponse.Ok(new StringBodyContent(content, encoding: Encoding.UTF8)));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = response.Content.Headers.ContentLength;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}