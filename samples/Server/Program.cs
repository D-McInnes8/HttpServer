using HttpServer;
using HttpServer.Response;
using HttpServer.Routing;
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
var httpServer = builder.Build();

httpServer.AddPipeline("TestPipeline", options =>
{
    options.AddPlugin<TestPlugin>();
});

httpServer.MapGet("/api/helloworld/", "TestPipeline", _ => HttpResponse.Ok("Hello, World!"));
httpServer.MapGet("/{*}", _ => HttpResponse.Ok("File not found!"));

await httpServer.StartAsync();