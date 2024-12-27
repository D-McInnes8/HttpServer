using Application;
using Application.Request;
using Application.Response;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

var builder = HttpServer.CreateBuilder(9999);
builder.Services.AddScoped<RoutingPlugin>();
builder.Services.AddSingleton<IRouteRegistry, RouteRegistry>();
var httpServer = builder.Build();

httpServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));
await httpServer.StartAsync();

/*var httpServer = new HttpServer(9999);
httpServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));
await httpServer.StartAsync();*/