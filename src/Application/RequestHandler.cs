using Application.Pipeline;
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

    public void AddRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        _routes.Add(new Route(method, path, handler));
    }

    public HttpResponse HandleRequest(HttpRequest httpRequest, IServiceProvider serviceProvider)
    {
        var requestPipeline = new RequestPipeline(serviceProvider);
        requestPipeline.AddPlugin<RoutingPlugin>();
        try
        {
            return requestPipeline.ExecuteAsync(httpRequest).GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return new HttpResponse(HttpResponseStatusCode.InternalServerError);
        }


        /*foreach (var route in _routes)
        {
            if (route.IsMatch(httpRequest.Method, httpRequest.Route))
            {
                try
                {
                    //var response = requestPipeline.ExecuteAsync(route.Handler);
                    return route.Handler(httpRequest);
                }
                catch (Exception ex)
                {
                    return new HttpResponse(HttpResponseStatusCode.InternalServerError, new HttpBody
                    {
                        ContentType = "text/plain",
                        Content = ex.Message
                    });
                }
            }
        }

        return new HttpResponse(HttpResponseStatusCode.NotFound);*/
    }
}