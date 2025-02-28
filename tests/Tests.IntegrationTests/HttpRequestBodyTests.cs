using HttpServer;
using HttpServer.Body;
using HttpServer.Request;
using HttpServer.Response;

namespace Tests.IntegrationTests;

public class HttpRequestBodyTests : IAsyncLifetime
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
    public async Task HttpRequestBody_RequestWithPlainTextBody_ShouldParseBody()
    {
        // Arrange
        HttpRequest? actual = null;
        _server.MapRoute(HttpRequestMethod.POST, "/test", ctx =>
        {
            actual = ctx.Request;
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
            Assert.Equal("Hello, World!", actual.Body?.Encoding.GetString(actual.Body.Content));
            Assert.Equal("text/plain; charset=utf-8", actual.ContentType?.Value);
        });
    }

    [Fact]
    public void HttpRequestBody_RequestWithLargeBody_ShouldProcessRequest()
    {
        // Arrange
        HttpRequest? actual = null;
        var expected = new string('A', 500_000);
        _server.MapRoute(HttpRequestMethod.POST, "/test", ctx =>
        {
            actual = ctx.Request;
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
            Assert.Equal(expected, actual.Body?.Encoding.GetString(actual.Body.Content));
        });
    }

    [Theory (Skip = "This test needs to be redesigned")]
    [InlineData(1, 10)]
    [InlineData(5, 100)]
    [InlineData(10, 1000)]
    [InlineData(100, 2000)]
    public async Task HttpRequestBody_ConcurrentRequests_ShouldProcessAll(int numberOfRequests, int bodySize)
    {
        // Arrange
        using var httpClient = new HttpClient();
        httpClient.BaseAddress =  new Uri($"http://localhost:{_server.Port}");
        var chars = Enumerable.Range(0, 26).Select(i => Convert.ToChar(i + 65)).ToArray();
        _server.MapRoute(HttpRequestMethod.POST, "/test", ctx => HttpResponse.Ok(ctx.Request.Body ?? new StringBodyContent(string.Empty)));
        
        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < numberOfRequests; i++)
        {
            //var client = new HttpClient();
            //client.BaseAddress =  new Uri($"http://localhost:{_server.Port}");
            var message = new HttpRequestMessage(HttpMethod.Post, "/test")
            {
                Content = new StringContent(new string(chars[i % chars.Length], bodySize))
            };
            tasks.Add(httpClient.SendAsync(message));
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