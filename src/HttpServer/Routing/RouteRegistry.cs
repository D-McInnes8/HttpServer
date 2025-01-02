using HttpServer.Request;
using HttpServer.Response;

namespace HttpServer.Routing;

public interface IRouteRegistry
{
    void AddRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler);
    RouteOld? MatchRoute(HttpRequest httpRequest);
}

public class RouteRegistry : IRouteRegistry
{
    private readonly List<RouteOld> _routes = new List<RouteOld>();

    public void AddRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        _routes.Add(new RouteOld(method, path, handler));
    }
    
    public RouteOld? MatchRoute(HttpRequest httpRequest)
    {
        return _routes.FirstOrDefault(route => route.IsMatch(httpRequest));
    }
}