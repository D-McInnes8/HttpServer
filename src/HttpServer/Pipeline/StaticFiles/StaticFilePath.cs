namespace HttpServer.Pipeline.StaticFiles;

public class StaticFilePath
{
    public required string VirtualPath { get; init; }
    public required string PhysicalPath { get; init; }
    public bool IsDirectory { get; init; }
}