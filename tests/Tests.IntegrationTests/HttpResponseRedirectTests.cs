using System.Net;
using HttpServer;
using HttpServer.Response;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseRedirectTests : IAsyncLifetime
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
    public async Task HttpResponseRedirect_Redirect_ShouldRedirect()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.Redirect("/redirect"));
        _server.MapGet("/redirect", _ => HttpResponse.Ok("Redirected!"));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Redirected!", content);
    }
    
    [Fact]
    public async Task HttpResponseRedirect_RedirectPermanent_ShouldRedirectPermanently()
    {
        // Arrange
        _server.MapGet("/test", _ => HttpResponse.MovePermanently("/redirect"));
        _server.MapGet("/redirect", _ => HttpResponse.Ok("Redirected permanently!"));
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Redirected permanently!", content);
    }
}