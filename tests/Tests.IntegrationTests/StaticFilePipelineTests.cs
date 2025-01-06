using System.Net;
using HttpServer;
using HttpServer.Routing.StaticFiles;
using Tests.IntegrationTests.TestPipelines;

namespace Tests.IntegrationTests;

public class StaticFilePipelineTests : IAsyncLifetime
{
    private readonly IHttpWebServer _server = HttpWebServer.CreateBuilder(9993).Build();
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
    
    // TODO: Rewrite these tests after the new interface for serving static files is implemented.
    
    [Fact]
    public async Task StaticFilePipeline_ShouldServeIndividualFile()
    {
        // Arrange
        _server.ServeFile("/file.txt", "SampleFiles/file.txt");
        
        // Act
        var response = await _httpClient.GetAsync("/file.txt");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equal("File content", actual);
    }
    
    [Fact]
    public async Task StaticFilePipeline_ShouldServeDirectory()
    {
        // Arrange
        _server.ServeDirectory("/files", "SampleFiles");

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
        _server.ServeFile("/file.txt", "SampleFiles/file.txt");
        _server.ServeDirectory("/files", "SampleFiles");

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
        _server.ServeFile("/file.txt", "SampleFiles/file.txt");

        // Act
        var response = await _httpClient.GetAsync("/invalid.txt");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task StaticFilePipeline_WithMultipleFiles_ShouldServeCorrectFile()
    {
        // Arrange
        _server.ServeFile("/file1.txt", "SampleFiles/file1.txt");
        _server.ServeFile("/file2.txt", "SampleFiles/file2.txt");
        
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
    
    [Theory]
    [InlineData("/file.txt", "SampleFiles/file.txt", "text/plain")]
    [InlineData("/file.json", "SampleFiles/file.json", "application/json")]
    [InlineData("/file.csv", "SampleFiles/file.csv", "text/csv")]
    [InlineData("/file.html", "SampleFiles/file.html", "text/html")]
    [InlineData("/file.xml", "SampleFiles/file.xml", "application/xml")]
    public async Task StaticFilePipeline_ShouldReturnCorrectContentType(string url, string filePath, string expectedContentType)
    {
        // Arrange
        _server.ServeFile(url, filePath);

        // Act
        var response = await _httpClient.GetAsync(url);

        // Assert
        Assert.Equal(expectedContentType, response.Content.Headers.ContentType?.MediaType);
    }
    
    [Fact]
    public async Task StaticFilePipeline_InvalidFilePath_ShouldReturnInternalServerError()
    {
        // Arrange
        _server.ServeFile("/file.txt", "invalid/path/to/file.txt");

        // Act
        var response = await _httpClient.GetAsync("/file.txt");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}