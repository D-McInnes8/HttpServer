using Application;
using Application.Pipeline;
using Application.Pipeline.Endpoints;
using Application.Pipeline.StaticFiles;
using Application.Request;
using Application.Response;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

var builder = HttpServer.CreateBuilder(9999);
//builder.Services.AddScoped<RoutingPlugin>();
//builder.Services.AddSingleton<IRouteRegistry, RouteRegistry>();
var httpServer = builder.Build();

/*httpServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));

httpServer.AddRequestPipeline<Testipeline>()
    .ConfigurePipeline(options =>
    {
        options.AddPlugin<RoutingPlugin>();
    })
    .UseDefaultHandler(async (request) =>
    {
        await Task.Yield();
        return new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
        {
            Content = "Hello, World!",
            ContentType = "text/plain"
        });
    });


httpServer.AddRequestPipeline<Testipeline>(pipelineName: "Simple Pipeline")
          .UseDefaultHandler(request => Task.FromResult(HttpResponse.Ok("Hello, World!")));

httpServer.AddRequestPipeline<Testipeline>(pipelineName: "Generic Request Pipeline")
          .ConfigurePlugins(pipeline =>
          {
              pipeline.ClearPlugins();
              pipeline.ResetPlugins();
              pipeline.RemovePlugin<RoutingPlugin>();
              pipeline.AddPlugin<RoutingPlugin>(priority: 1);
          })
          .UseRouter<RoutingPlugin>()
          .UseDefaultHandler(request => Task.FromResult(HttpResponse.Ok()));

httpServer.AddPipeline()
          .UseRouter<RoutingPlugin>()
          .UseRequestHandler<RoutingPlugin>()
          .WithName("Simple Pipeline")
          .WithPriority(1)
          .WithPathPrefix("/api")
          .ConfigurePipeline(options =>
          {
                options.AddPlugin<RoutingPlugin>();
          });

httpServer.AddPipeline("Simple Pipeline", pipelineBuilder =>
{
    pipelineBuilder.UseRouter<RoutingPlugin>()
        .UseRequestHandler<RoutingPlugin>()
        .WithPriority(1)
        .WithPathPrefix("/api")
        .ConfigurePlugins(options =>
        {
            options.AddPlugin<RoutingPlugin>();
        });
});

httpServer.AddStaticFilePipeline("Static File Pipeline", options =>
{
    options.Priority = 2;
    options.PipelineName = "";
    options.PathPrefix = "/";
    options.ServeDirectory("/", "wwwroot");
    
    options.ServeDirectory("/", "wwwroot")
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
           });
});*/

/*httpServer.AddHttpEndpointPipeline("Response Pipeline", priority: 1)
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


httpServer.AddPipeline(options =>
{
    options.Name = "Test Pipeline";
});

httpServer.AddEndpointPipeline(options =>
{
    options.Priority = 1;
    options.Name = "Test Endpoint Pipeline";
    options.MapRoute(HttpRequestMethod.GET, "/test", (_) => HttpResponse.Ok("Hello World, from a main handler!"));
    options.MapRoute(HttpRequestMethod.GET, "/error", (_) => throw new Exception("AN EXCEPTION!!"));
});

httpServer.AddStaticFilePipeline(options =>
{
    options.Priority = 2;
    options.ServeFile("/sample.txt", "/Users/dmcinnes/Documents/PieceTableNotes.txt");
    options.ServeDirectory("/", "/Users/dmcinnes/Documents/Projects/HttpServer/wwwroot");
});

await httpServer.StartAsync();

/*var httpServer = new HttpServer(9999);
httpServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));
await httpServer.StartAsync();*/