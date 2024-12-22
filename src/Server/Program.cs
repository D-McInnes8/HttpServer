using Application;
using Application.Request;
using Application.Response;

var httpServer = new HttpServer(9999);
httpServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));
await httpServer.StartAsync();