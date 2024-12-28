using Application;
using Application.Pipeline;
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

/*httpServer.AddRequestPipeline<RequestPipeline2>()
    .ConfigurePipeline(options =>
    {
        options.AddPlugin<RoutingPlugin>();
    });

httpServer.AddHttpEndpointPipeline("Response Pipeline", priority: 1)
    .ConfigureRequestPipeline(options =>
    {
        options.UsePlugin<RoutingPlugin>();
        options.UseRateLimitingPlugin();
        options.UseAuthenticationPlugin();
        options.RemovePlugin<RateLimitingPlugin>();

        options.AddPlugin<RoutingPlugin>();
        options.AddPlugin<LoggingPlugin>();
    })
    .ConfigureRoutes(routes =>
    {
        routes.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
        {
            Content = "Hello, World!",
            ContentType = "text/plain"
        }));
    });

httpServer.AddStaticFilePipeline(priority: 2)
    .ServeDirectory("/", "wwwroot")
    .ServeDirectory("/images", "wwwroot/images")
    .ServeFile("/favicon.ico", "wwwroot/favicon.ico")
    .ConfigureRequestPipeline(options =>
    {
        options.SetPluginPriority<StaticFilePlugin>(priority: 1);
        options.SetPluginPriority<LoggingPlugin>(priority: 2);
        options.AddPlugin<StaticFilePlugin>();
    })
    .ConfigureFileServerOptions(options =>
    {
        options.DefaultFile = "index.html";
    });*/

await httpServer.StartAsync();

/*var httpServer = new HttpServer(9999);
httpServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));
await httpServer.StartAsync();*/