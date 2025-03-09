using System.Net.Http.Json;
using HttpServer;
using HttpServer.Body;
using HttpServer.Response;
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
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!")
            };
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
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!"),
                new StringBodyContent("Hello, 世界!"),
                new StringBodyContent("こんにちは世界")
            };
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        Assert.Collection(parts.Contents,
            part => Assert.Equal("Hello, World!", part.ReadAsStringAsync().Result),
            part => Assert.Equal("Hello, 世界!", part.ReadAsStringAsync().GetAwaiter().GetResult()),
            part => Assert.Equal("こんにちは世界", part.ReadAsStringAsync().GetAwaiter().GetResult()));
    }

    [Fact]
    public async Task HttpResponseBodyMultipartFormData_PlainTextContent_ShouldSetContentTypeHeader()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!")
            };
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
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!")
            };
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
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!")
            };
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
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03])
            };
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
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03])
            };
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
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03])
            };
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
    public async Task HttpResponseMultipartFormData_MultipleParts_ShouldContainMultipleParts()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!"),
                new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]),
                new JsonBodyContent<int[]>([1, 2, 3, 4, 5])
            };
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();

        Assert.Collection(parts.Contents,
            part => Assert.Equal("Hello, World!", part.ReadAsStringAsync().GetAwaiter().GetResult()),
            part => Assert.Equal(new byte[] { 0x00, 0x01, 0x02, 0x03 }, part.ReadAsByteArrayAsync().GetAwaiter().GetResult()),
            part => Assert.Equal([1,2,3,4,5], part.ReadFromJsonAsync<int[]>().GetAwaiter().GetResult()!));
    }
    
    [Fact]
    public async Task HttpResponseMultipartFormData_MultipleParts_ShouldSetContentTypeHeader()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!"),
                new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03])
            };
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
    
    [Fact]
    public async Task HttpResponseBodyMultipartFormData_MultipleParts_ShouldSetContentDisposition()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent("Hello, World!"),
                new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03])
            };
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        Assert.All(parts.Contents, part => Assert.Equal("form-data", part.Headers.ContentDisposition?.DispositionType));
    }

    [Fact]
    public async Task HttpResponseBodyMultipartFormData_MultipleParts_ShouldSetPartNameHeader()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary")
            {
                { "binary", new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]) },
                { "json", new JsonBodyContent<int[]>([1, 2, 3, 4, 5]) }
            };
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        Assert.Collection(parts.Contents,
            part => Assert.Equal("binary", part.Headers.ContentDisposition?.Name),
            part => Assert.Equal("json", part.Headers.ContentDisposition?.Name));
    }
    
    [Fact]
    public async Task HttpResponseBodyMultipartFormData_MultipleParts_ShouldSetFilenameHeader()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary")
            {
                { "binary", "file.bin", new ByteArrayBodyContent([0x00, 0x01, 0x02, 0x03]) },
                { "json", "data.json", new JsonBodyContent<int[]>([1, 2, 3, 4, 5]) }
            };
            return HttpResponse.Ok(response);
        });
        
        // Act
        var response = await _httpClient.GetAsync("/test");
        
        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        Assert.Collection(parts.Contents,
            part => Assert.Equal("file.bin", part.Headers.ContentDisposition?.FileName),
            part => Assert.Equal("data.json", part.Headers.ContentDisposition?.FileName));
    }
    
    [Fact]
    public async Task HttpResponseBodyMultipartFormData_NestedMultipartContent_ShouldContainNestedParts()
    {
        // Arrange
        _server.MapGet("/test", _ =>
        {
            var nestedContent = new MultipartFormDataBodyContent("nested-boundary")
            {
                new StringBodyContent("Nested part")
            };
            var response = new MultipartFormDataBodyContent("boundary")
            {
                nestedContent
            };
            return HttpResponse.Ok(response);
        });

        // Act
        var response = await _httpClient.GetAsync("/test");

        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var nestedParts = await Assert.Single(parts.Contents).ReadAsMultipartAsync();
        var actual = Assert.Single(nestedParts.Contents);
        Assert.Equal("Nested part", await actual.ReadAsStringAsync());
    }
    
    [Fact]
    public async Task HttpResponseBodyMultipartFormData_LargeContent_ShouldContainAllParts()
    {
        // Arrange
        var expected = new string('a', 1000000);
        _server.MapGet("/test", _ =>
        {
            var response = new MultipartFormDataBodyContent("boundary")
            {
                new StringBodyContent(expected)
            };
            return HttpResponse.Ok(response);
        });

        // Act
        var response = await _httpClient.GetAsync("/test");

        // Assert
        var parts = await response.Content.ReadAsMultipartAsync();
        var actual = Assert.Single(parts.Contents);
        Assert.Equal(expected, await actual.ReadAsStringAsync());
    }
}