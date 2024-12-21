using Application.Request;
using Application.Response;

namespace Application;

public class RequestHandler
{
    public HttpResponse HandleRequest(HttpRequest httpRequest)
    {
        return new HttpResponse(HttpResponseStatusCode.OK);
    }
}