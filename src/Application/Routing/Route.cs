using System.Diagnostics.CodeAnalysis;
using Application.Request;
using Application.Response;

namespace Application.Routing;

public class Route
{
    public required string Path { get; init; }
    public required HttpRequestMethod Method { get; init; }
    public required Func<HttpRequest, HttpResponse> Handler { get; init; }
    
    public Route()
    {
    }

    [SetsRequiredMembers]
    public Route(HttpRequestMethod method, string path)
    {
        Method = method;
        Path = path;
        Handler = _ => new HttpResponse(HttpResponseStatusCode.OK);
    }

    [SetsRequiredMembers]
    public Route(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        Method = method;
        Path = path;
        Handler = handler;
    }
    
    public bool IsMatch(HttpRequest httpRequest) => IsMatch(httpRequest.Method, httpRequest.Path);
    
    public bool IsMatch(HttpRequestMethod method, string path)
    {
        return Method == method && Path == path;
    }
}