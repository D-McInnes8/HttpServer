namespace HttpServer.Headers;

public class ContentDisposition
{
    public string? FileName { get; }
    public string? Name { get; }

    public ContentDisposition(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
    }
    
    public ContentDisposition(string fileName, string name)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(name);
        
        FileName = fileName;
        Name = name;
    }
}