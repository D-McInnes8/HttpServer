using System.Diagnostics.CodeAnalysis;
using Application.Request;

namespace Application.Routing;

public class Route
{
    public required string Path { get; init; }
    public required HttpRequestMethod Method { get; init; }
    
    public Route()
    {
    }

    [SetsRequiredMembers]
    public Route(HttpRequestMethod method, string path)
    {
        Method = method;
        Path = path;
    }
    
    public bool IsMatch(HttpRequest httpRequest) => IsMatch(httpRequest.Method, httpRequest.Path);
    
    public bool IsMatch(HttpRequestMethod method, string path)
    {
        return Method == method && Path == path;
    }
}