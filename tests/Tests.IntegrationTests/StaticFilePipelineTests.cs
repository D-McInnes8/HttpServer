using System.Net;
using HttpServer;
using HttpServer.Pipeline.StaticFiles;
using Tests.IntegrationTests.TestPipelines;

namespace Tests.IntegrationTests;

public class StaticFilePipelineTests : IAsyncLifetime
{
    private readonly HttpWebWebServer _httpWebWebServer = HttpWebWebServer.CreateBuilder(9993).Build();
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
    
    [Fact]
    public async Task StaticFilePipeline_ShouldServeIndividualFile()
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeFile("/file.txt", "path/to/file.txt");
        });

        // Act
        var response = await _httpClient.GetAsync("/file.txt");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("File content", content);
    }
    
    [Fact]
    public async Task StaticFilePipeline_ShouldServeDirectory()
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeDirectory("/files", "path/to/files");
        });

        // Act
        var response = await _httpClient.GetAsync("/files/file1.txt");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("File1 content", content);
    }
    
    [Fact]
    public async Task StaticFilePipeline_ShouldServeFileAndDirectory()
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeFile("/file.txt", "path/to/file.txt");
            options.ServeDirectory("/files", "path/to/files");
        });

        // Act
        var fileResponse = await _httpClient.GetAsync("/file.txt");
        var dirResponse = await _httpClient.GetAsync("/files/file1.txt");

        // Assert
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);
        var fileContent = await fileResponse.Content.ReadAsStringAsync();
        Assert.Equal("File content", fileContent);

        Assert.Equal(HttpStatusCode.OK, dirResponse.StatusCode);
        var dirContent = await dirResponse.Content.ReadAsStringAsync();
        Assert.Equal("File1 content", dirContent);
    }
    
    [Fact]
    public async Task StaticFilePipeline_ShouldReturnNotFoundForInvalidPath()
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeFile("/file.txt", "path/to/file.txt");
        });

        // Act
        var response = await _httpClient.GetAsync("/invalid.txt");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task StaticFilePipeline_WithMultipleFiles_ShouldServeCorrectFile()
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeFile("/file1.txt", "path/to/file1.txt");
            options.ServeFile("/file2.txt", "path/to/file2.txt");
        });

        // Act
        var file1Response = await _httpClient.GetAsync("/file1.txt");
        var file2Response = await _httpClient.GetAsync("/file2.txt");

        // Assert
        Assert.Equal(HttpStatusCode.OK, file1Response.StatusCode);
        var file1Content = await file1Response.Content.ReadAsStringAsync();
        Assert.Equal("File1 content", file1Content);

        Assert.Equal(HttpStatusCode.OK, file2Response.StatusCode);
        var file2Content = await file2Response.Content.ReadAsStringAsync();
        Assert.Equal("File2 content", file2Content);
    }

    [Fact]
    public async Task StaticFilePipeline_WithCustomPlugins_ShouldExecutePlugins()
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeFile("/file.txt", "path/to/file.txt");
            options.UseRouter<TestRouter>();
        });
        
        // Act
        using var response = await _httpClient.GetAsync("/file.txt");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
    
    [Theory]
    [InlineData("/file.txt", "path/to/file.txt", "text/plain")]
    [InlineData("/file.json", "path/to/file.json", "application/json")]
    [InlineData("/file.csv", "path/to/file.csv", "text/csv")]
    [InlineData("/file.html", "path/to/file.html", "text/html")]
    [InlineData("/file.xml", "path/to/file.xml", "application/xml")]
    public async Task StaticFilePipeline_ShouldReturnCorrectContentType(string url, string filePath, string expectedContentType)
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeFile(url, filePath);
        });

        // Act
        var response = await _httpClient.GetAsync(url);

        // Assert
        Assert.Equal(expectedContentType, response.Content.Headers.ContentType?.MediaType);
    }
    
    [Fact]
    public async Task StaticFilePipeline_ShouldReturnBadRequestForInvalidFilePath()
    {
        // Arrange
        _httpWebWebServer.AddStaticFilePipeline(options =>
        {
            options.ServeFile("/file.txt", "invalid/path/to/file.txt");
        });

        // Act
        var response = await _httpClient.GetAsync("/file.txt");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}