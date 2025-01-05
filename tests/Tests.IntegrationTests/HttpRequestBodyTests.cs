using HttpServer;
using HttpServer.Request;
using HttpServer.Response;

namespace Tests.IntegrationTests;

public class HttpRequestBodyTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9997).Build();
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
    
    [Fact]
    public async Task HttpRequestBody_RequestWithPlainTextBody_ShouldParseBody()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapRoute(HttpRequestMethod.POST, "/test", request =>
        {
            actual = request;
            return HttpResponse.Ok();
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
            Assert.Equal("text/plain; charset=utf-8", actual.ContentType?.Value);
            //Assert.Equal("text/plain; charset=utf-8", actual.ContentType);
        });
    }

    [Fact]
    public void HttpRequestBody_RequestWithLargeBody_ShouldProcessRequest()
    {
        // Arrange
        HttpRequest? actual = null;
        var expected = new string('A', 500_000);
        _server.MapRoute(HttpRequestMethod.POST, "/test", request =>
        {
            actual = request;
            return HttpResponse.Ok();
        });
        
        // Act
        var message = new HttpRequestMessage(HttpMethod.Post, "/test")
        {
            Content = new StringContent(expected)
        };
        var response = _httpClient.Send(message);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual.Body);
        });
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(5, 100)]
    [InlineData(10, 1000)]
    [InlineData(100, 2000)]
    public async Task HttpRequestBody_ConcurrentRequests_ShouldProcessAll(int numberOfRequests, int bodySize)
    {
        // Arrange
        var chars = Enumerable.Range(0, 26).Select(i => Convert.ToChar(i + 65)).ToArray();
        _server.MapRoute(HttpRequestMethod.POST, "/test", request => HttpResponse.Ok(request.Body ?? string.Empty));
        
        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < numberOfRequests; i++)
        {
            var client = new HttpClient();
            client.BaseAddress =  new Uri($"http://localhost:{_server.Port}");
            var message = new HttpRequestMessage(HttpMethod.Post, "/test")
            {
                Content = new StringContent(new string(chars[i % chars.Length], bodySize))
            };
            tasks.Add(client.SendAsync(message));
        }
        var actual = await Task.WhenAll(tasks);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.All(actual, r => Assert.True(r.IsSuccessStatusCode));
            Assert.Equal(numberOfRequests, actual.Length);
        });
        
        for (int i = 0; i < numberOfRequests; i++)
        {
            var response = actual[i];
            var expectedBody = new string(chars[i % chars.Length], bodySize);
            var actualBody = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedBody, actualBody);
        }
    }
}