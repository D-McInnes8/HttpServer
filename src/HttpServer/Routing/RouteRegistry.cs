using HttpServer.Request;
using HttpServer.Response;

namespace HttpServer.Routing;

public interface IRouteRegistry
{
    void AddRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler);
    Route? MatchRoute(HttpRequest httpRequest);
}

public class RouteRegistry : IRouteRegistry
{
    private readonly List<Route> _routes = new List<Route>();

    public void AddRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        _routes.Add(new Route(method, path, handler));
    }
    
    public Route? MatchRoute(HttpRequest httpRequest)
    {
        return _routes.FirstOrDefault(route => route.IsMatch(httpRequest));
    }
}