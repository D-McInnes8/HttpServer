using System.Diagnostics.CodeAnalysis;
using HttpServer.Pipeline;
using HttpServer.Request;
using HttpServer.Response;

namespace HttpServer.Routing;

public class RouteOld : IPipelineData
{
    public required string Path { get; init; }
    public required HttpRequestMethod Method { get; init; }
    public required Func<HttpRequest, HttpResponse> Handler { get; init; }
    
    public RouteOld()
    {
    }

    [SetsRequiredMembers]
    public RouteOld(HttpRequestMethod method, string path)
    {
        Method = method;
        Path = path;
        Handler = _ => new HttpResponse(HttpResponseStatusCode.OK);
    }

    [SetsRequiredMembers]
    public RouteOld(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        Method = method;
        Path = path;
        Handler = handler;
    }
    
    public bool IsMatch(HttpRequest httpRequest) => IsMatch(httpRequest.Method, httpRequest.Route);
    
    public bool IsMatch(HttpRequestMethod method, string path)
    {
        return Method == method && Path == path;
    }
}