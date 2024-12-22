using Application.Request;
using Application.Response;
using Application.Routing;

namespace Application;

public class RequestHandler
{
    private readonly List<Route> _routes;
    
    public RequestHandler()
    {
        _routes = new List<Route>();
    }

    public RequestHandler(ICollection<Route> routes)
    {
        _routes = routes.ToList();
    }

    public void AddRoute(HttpRequestMethod method, string path)
    {
        _routes.Add(new Route(method, path));
    }

    public HttpResponse HandleRequest(HttpRequest httpRequest)
    {
        if (_routes.Any(route => route.IsMatch(httpRequest.Method, httpRequest.Route)))
        {
            return new HttpResponse(HttpResponseStatusCode.OK, new HttpBody
            {
                ContentType = "text/plain",
                Content = "Hello, World!"
            });
        }

        return new HttpResponse(HttpResponseStatusCode.NotFound);
    }
}