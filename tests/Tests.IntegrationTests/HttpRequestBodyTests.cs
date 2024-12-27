using Application;
using Application.Request;
using Application.Response;
using NSubstitute;

namespace Tests.IntegrationTests;

public class HttpRequestBodyTests : IAsyncLifetime
{
    private readonly HttpServer _httpServer = HttpServer.CreateBuilder(9997).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        _httpClient.BaseAddress = new Uri($"http://localhost:{_httpServer.Port}");
        await _httpServer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _httpServer.StopAsync();
    }
    
    [Fact]
    public async Task HttpRequestBody_RequestWithPlainTextBody_ShouldParseBody()
    {
        // Arrange
        HttpRequest? actual = null;
        _httpServer.AddRoute(HttpRequestMethod.POST, "/test", request =>
        {
            actual = request;
            return new HttpResponse(HttpResponseStatusCode.OK);
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Post, "/test")
        {
            Content = new StringContent("Hello, World!")
        };
        _ = await _httpClient.SendAsync(message);
        
        // Assert
        Assert.NotNull(actual);
        Assert.Multiple(() =>
        {
            Assert.Equal("Hello, World!", actual.Body);
            Assert.Equal("text/plain; charset=utf-8", actual.ContentType);
        });
    }
}