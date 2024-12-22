using Application;
using Application.Request;

var httpServer = new HttpServer(9999);
httpServer.AddRoute(HttpRequestMethod.GET, "/");
await httpServer.StartAsync();