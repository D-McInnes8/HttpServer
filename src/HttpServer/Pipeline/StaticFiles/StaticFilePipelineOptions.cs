using System.Diagnostics.CodeAnalysis;

namespace HttpServer.Pipeline.StaticFiles;

/// <summary>
/// Options for the static file pipeline.
/// </summary>
/// <param name="services">The <see cref="IServiceProvider"/> associated with the web server.</param>
public class StaticFilePipelineOptions(IServiceProvider services) : RequestPipelineBuilderOptions(services)
{
    /// <summary>
    /// The list of routes to be served by the static file pipeline.
    /// </summary>
    public ICollection<StaticFileRoute> Routes { get; } = new List<StaticFileRoute>();
    
    /// <summary>
    /// Configure the static file pipeline to serve the specified file.
    /// </summary>
    /// <param name="virtualPath">The url to be used to access the file on the server.</param>
    /// <param name="physicalPath">The physical path of the file on the machine the web server is running on.</param>
    public void ServeFile(string virtualPath, string physicalPath)
    {
        Routes.Add(new StaticFileRoute(virtualPath, physicalPath, isDirectory: false));
    }

    /// <summary>
    /// Configure the static file pipeline to serve files from the specified directory.
    /// </summary>
    /// <param name="virtualPath">The url to be used to access the directory on the server.</param>
    /// <param name="physicalPath">The physical path of the directory on the machine the web server is running on.</param>
    public void ServeDirectory(string virtualPath, string physicalPath)
    {
        Routes.Add(new StaticFileRoute(virtualPath, physicalPath, isDirectory: true));
    }
}

public class StaticFileRoute : IPipelineData
{
    public required string VirtualPath { get; init; }
    public required string PhysicalPath { get; init; }
    public bool IsDirectory { get; init; }

    public StaticFileRoute()
    {
    }
    
    [SetsRequiredMembers]
    public StaticFileRoute(string virtualPath, string physicalPath, bool isDirectory)
    {
        VirtualPath = virtualPath;
        PhysicalPath = physicalPath;
        IsDirectory = isDirectory;
    }
}