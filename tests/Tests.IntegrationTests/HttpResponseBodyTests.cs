using Application;
using Application.Pipeline.Endpoints;
using Application.Request;
using Application.Response;

namespace Tests.IntegrationTests;

public class HttpResponseBodyTests : IAsyncLifetime
{
    private readonly HttpWebWebServer _httpWebWebServer = HttpWebWebServer.CreateBuilder(9996).Build();
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task InitializeAsync()
    {
        _httpClient.BaseAddress = new Uri($"http://localhost:{_httpWebWebServer.Port}");
        await _httpWebWebServer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _httpWebWebServer.StopAsync();
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
        _httpWebWebServer.AddEndpointPipeline(options =>
        {
            options.MapRoute(HttpRequestMethod.GET, "/test", _ => HttpResponse.Ok(expected));
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        var actual = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(expected, actual);
    }
}