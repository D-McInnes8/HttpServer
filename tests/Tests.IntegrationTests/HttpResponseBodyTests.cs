using Application;
using Application.Request;
using Application.Response;

namespace Tests.IntegrationTests;

public class HttpResponseBodyTests: IAsyncLifetime
{
    private readonly HttpServer _httpServer = new HttpServer(9996);
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
    
    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("Hello, World! 1234567890")]
    [InlineData("Hello, World! With Special Characters!@#")]
    [InlineData("Hello, World! With Spaces")]
    [InlineData("Hello, World! With New Line\n")]
    [InlineData("Hello, World! With Carriage Return\r")]
    [InlineData("Hello, World! With Tab\t")]
    [InlineData("Hello, World! With Backspace\b")]
    [InlineData("Hello, World! With Form Feed\f")]
    [InlineData("Hello, World! With Vertical Tab\v")]
    public async Task HttpResponseBody_ResponseWithPlainTextBody_ShouldReturnBody(string expected)
    {
        // Arrange
        _httpServer.AddRoute(HttpRequestMethod.GET, "/test", (_) 
            => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody("text/plain", expected)));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(expected, actual);
    }
}