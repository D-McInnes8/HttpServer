using System.Diagnostics;
using Application.Pipeline;
using Application.Pipeline.Registry;
using Application.Request;
using Application.Request.Parser;
using Application.Response;
using Application.Response.Internal;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public interface IHttpServer
{
    IServiceProvider Services { get; }
    int Port { get; }
    Uri LocalEndpoint { get; }
    
    Task StartAsync();
    Task StopAsync();
}

public class HttpServer : IHttpServer
{
    public IServiceProvider Services { get; }
    public int Port => _tcpServer.Port;
    public Uri LocalEndpoint => _tcpServer.LocalEndpoint;
    
    private readonly TcpServer _tcpServer;
    private readonly RequestHandler _requestHandler;
    private readonly IPipelineRegistry _pipelineRegistry;

    /*private readonly Route[] _routes =
    [
        new Route { Method = HttpRequestMethod.GET, Path = "/" },
        new Route { Method = HttpRequestMethod.GET, Path = "/helloworld/" }
    ];*/

    /*public HttpServer() : this(9999)
    {
    }*/

    /*public HttpServer(int port)
    {
        Debug.Assert(port >= 0);
        _tcpServer = new TcpServer(port, HandleRequest);
        _requestHandler = new RequestHandler();

        var services = new ServiceCollection();
        services.AddSingleton<IRouteRegistry, RouteRegistry>();
        services.AddScoped<RoutingPlugin>();
        Services = services.BuildServiceProvider();
    }*/

    internal HttpServer(int port, IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        
        ArgumentOutOfRangeException.ThrowIfNegative(port, nameof(port));
        _tcpServer = new TcpServer(port, HandleRequest);
        _pipelineRegistry = serviceProvider.GetRequiredService<IPipelineRegistry>();
        _requestHandler = new RequestHandler(_pipelineRegistry);
    }

    public async Task StartAsync()
    {
        await _tcpServer.StartAsync();
    }

    public async Task StopAsync()
    {
        await _tcpServer.StopAsync();
    }

    public void AddRoute(HttpRequestMethod method, string path, Func<HttpRequest, HttpResponse> handler)
    {
        //_requestHandler.AddRoute(method, path, handler);
        var routeRegistry = Services.GetRequiredService<IRouteRegistry>();
        routeRegistry.AddRoute(method, path, handler);
    }

    private string HandleRequest(string request)
    {
        using var scope = Services.CreateScope();
        var httpRequest = HttpRequestParser.Parse(request);
        var httpResponse = _requestHandler.HandleRequest(httpRequest, scope.ServiceProvider);
        var response = HttpResponseWriter.WriteResponse(httpResponse);
        return response;
    }
    
    public static IHttpServerBuilder CreateBuilder(int port) => new HttpServerBuilder(port);
    
    public IHttpServer AddPipeline(Action<RequestPipelineBuilderOptions> configure)
    {
        var pipelineOptions = new RequestPipelineBuilderOptions()
        {
            Name = $"Pipeline {Guid.NewGuid():N}"
        };
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }

    public IHttpServer AddPipeline(string pipelineName, Action<RequestPipelineBuilderOptions> configure)
    {
        var pipelineOptions = new RequestPipelineBuilderOptions
        {
            Name = pipelineName
        };
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }
}