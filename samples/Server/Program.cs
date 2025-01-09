using HttpServer;
using HttpServer.Response;
using HttpServer.Routing;
using HttpServer.Routing.StaticFiles;
using Server;

var builder = HttpWebServer.CreateBuilder(9999);
var server = builder.Build();

server.AddPipeline("TestPipeline", options =>
{
    options.AddPlugin<TestPlugin>();
});

server.MapGet("/api/helloworld/", "TestPipeline", _ => HttpResponse.Ok("Hello, World!"));
server.ServeDirectory("/", "wwwroot");

await server.StartAsync();