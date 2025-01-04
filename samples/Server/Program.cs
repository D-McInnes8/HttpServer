using HttpServer;
using HttpServer.Response;
using Server;

/*var router = new RoutingRadixTree<int?>();
//router.AddRoute(new Route("/api/v1/users/{userId}/posts", HttpRequestMethod.GET), 1);
//router.AddRoute(new Route("/api/v1/users/{userId}/posts/{postId}/comments/{commentId}", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/users", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/users/{userId}", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/users/{userId}/posts", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/users/{userId}/posts/{postId}", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/users/{userId}/posts/{postId}/comments", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/users/{userId}/posts/{postId}/comments/{commentId}", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/comments", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/comments/{commentId}", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/posts", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v1/posts/{postId}", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v2/users", HttpRequestMethod.GET), 1);
router.AddRoute(new Route("/api/v2/users/{userId}", HttpRequestMethod.GET), 1);

//router.AddRoute(new Route("/api/v1/users", HttpRequestMethod.GET), 1);
//router.AddRoute(new Route("/api/v1/users/{userId}", HttpRequestMethod.GET), 1);

Console.WriteLine(router.Print());

return;*/

var builder = HttpWebServer.CreateBuilder(9999);
//builder.Services.AddScoped<RoutingPlugin>();
//builder.Services.AddSingleton<IRouteRegistry, RouteRegistry>();
var httpServer = builder.Build();

/*httpWebServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));

httpWebServer.AddRequestPipeline<Testipeline>()
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


httpWebServer.AddRequestPipeline<Testipeline>(pipelineName: "Simple Pipeline")
          .UseDefaultHandler(request => Task.FromResult(HttpResponse.Ok("Hello, World!")));

httpWebServer.AddRequestPipeline<Testipeline>(pipelineName: "Generic Request Pipeline")
          .ConfigurePlugins(pipeline =>
          {
              pipeline.ClearPlugins();
              pipeline.ResetPlugins();
              pipeline.RemovePlugin<RoutingPlugin>();
              pipeline.AddPlugin<RoutingPlugin>(priority: 1);
          })
          .UseRouter<RoutingPlugin>()
          .UseDefaultHandler(request => Task.FromResult(HttpResponse.Ok()));

httpWebServer.AddPipeline()
          .UseRouter<RoutingPlugin>()
          .UseRequestHandler<RoutingPlugin>()
          .WithName("Simple Pipeline")
          .WithPriority(1)
          .WithPathPrefix("/api")
          .ConfigurePipeline(options =>
          {
                options.AddPlugin<RoutingPlugin>();
          });

httpWebServer.AddPipeline("Simple Pipeline", pipelineBuilder =>
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

httpWebServer.AddStaticFilePipeline("Static File Pipeline", options =>
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

/*httpWebServer.AddHttpEndpointPipeline("Response Pipeline", priority: 1)
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

httpWebServer.AddStaticFilePipeline(priority: 2)
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


/*httpServer.AddPipeline(options =>
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
});*/

httpServer.AddPipeline("TestPipeline", options =>
{
    options.AddPlugin<TestPlugin>();
});

httpServer.MapGet("/api/helloworld/", "TestPipeline", _ => HttpResponse.Ok("Hello, World!"));
httpServer.MapGet("/{*}", _ => HttpResponse.Ok("File not found!"));

await httpServer.StartAsync();

/*var httpWebServer = new HttpWebWebServer(9999);
httpWebServer.AddRoute(HttpRequestMethod.GET, "/", (_) => new HttpResponse(HttpResponseStatusCode.OK, new HttpBody()
{
    Content = "Hello, World!",
    ContentType = "text/plain"
}));
await httpWebServer.StartAsync();*/