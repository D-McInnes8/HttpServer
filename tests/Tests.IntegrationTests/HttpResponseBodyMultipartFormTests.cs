using HttpServer;
using HttpServer.Response;
using HttpServer.Response.Body;
using HttpServer.Routing;

namespace Tests.IntegrationTests;

public class HttpResponseBodyMultipartFormTests : IAsyncLifetime
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
    public async Task HttpResponseBodyMultipartFormData_EmptyContent_ShouldContainNoParts()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var actual = await response.Content.ReadAsMultipartAsync();
        Assert.Empty(actual.Contents);
    }
    
    [Theory]
    [InlineData("boundary")]
    [InlineData("another-boundary")]
    [InlineData("1234567890")]
    [InlineData("abcdefghijklmnopqrstuvwxyz")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData("0123456789")]
    [InlineData("--boundary--")]
    [InlineData("-?@#$%^&*()_+")]
    public async Task HttpResponseBodyMultipartFormData_Boundary_ShouldMatchContentType(string expected)
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent(expected);
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        Assert.Equal(expected, response.Content.Headers.ContentType?.Parameters.Single(x => x.Name == "boundary").Value);
    }
    
    [Fact]
    public async Task HttpResponseBodyMultipartFormData_PlainTextContent_ShouldContainOnePart()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new StringBodyContent("Hello, World!"));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal("Hello, World!", await actual.ReadAsStringAsync());
    }
    
    [Fact]
    public async Task HttpResponseBodyMultipartFormData_PlainTextContent_ShouldContainMultipleParts()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new StringBodyContent("Hello, World!"));
            response.Add(new StringBodyContent("Hello, 世界!"));
            response.Add(new StringBodyContent("こんにちは世界"));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        Assert.Collection(parts.Contents,
            part => Assert.Equal("Hello, World!", part.ReadAsStringAsync().Result),
            part => Assert.Equal("Hello, 世界!", part.ReadAsStringAsync().Result),
            part => Assert.Equal("こんにちは世界", part.ReadAsStringAsync().Result));
    }

    [Fact]
    public async Task HttpResponseBodyMultipartFormData_PlainTextContent_ShouldSetContentTypeHeader()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new StringBodyContent("Hello, World!"));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal("text/plain", actual.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task HttpResponseMultipartFormData_PlainTextContent_ShouldSetContentTypeCharset()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new StringBodyContent("Hello, World!"));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal("us-ascii", actual.Headers.ContentType?.CharSet);
    }
    
    [Fact]
    public async Task HttpResponseMultipartFormData_PlainTextContent_ShouldSetContentDisposition()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new StringBodyContent("Hello, World!"));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal("form-data", actual.Headers.ContentDisposition?.DispositionType);
    }
    
    [Fact]
    public async Task HttpResponseMultipartFormData_BinaryContent_ShouldContainOnePart()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal(new byte[] { 0x00, 0x01, 0x02, 0x03 }, await actual.ReadAsByteArrayAsync());
    }
    
    [Fact]
    public async Task HttpResponseMultipartFormData_BinaryContent_ShouldSetContentTypeHeader()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal("application/octet-stream", actual.Headers.ContentType?.MediaType);
    }
    
    [Fact]
    public async Task HttpResponseMultipartFormData_BinaryContent_ShouldSetContentDisposition()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal("form-data", actual.Headers.ContentDisposition?.DispositionType);
    }
    
    [Fact]
    public async Task HttpResponseMultipartFormData_MultipleContents_ShouldContainMultipleParts()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new StringBodyContent("Hello, World!"));
            response.Add(new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        Assert.Collection(parts.Contents,
            part => Assert.Equal("Hello, World!", part.ReadAsStringAsync().Result),
            part => Assert.Equal(new byte[] { 0x00, 0x01, 0x02, 0x03 }, part.ReadAsByteArrayAsync().Result));
    }
    
    [Fact]
    public async Task HttpResponseMutlipartFormData_MultipleContents_ShouldSetContentTypeHeader()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary");
            response.Add(new StringBodyContent("Hello, World!"));
            response.Add(new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]));
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        Assert.Collection(parts.Contents,
            part => Assert.Equal("text/plain", part.Headers.ContentType?.MediaType),
            part => Assert.Equal("application/octet-stream", part.Headers.ContentType?.MediaType));
    }
}