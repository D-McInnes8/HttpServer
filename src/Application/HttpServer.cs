using Application.Pipeline;
using Application.Pipeline.Registry;
using Application.Request.Parser;
using Application.Response.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public interface IHttpServer
{
    IServiceProvider Services { get; }
    int Port { get; }
    Uri LocalEndpoint { get; }
    
    Task StartAsync();
    Task StopAsync();

    public IHttpServer AddPipeline<TOptions>(Action<TOptions> configure) where TOptions : RequestPipelineBuilderOptions;
    IHttpServer AddPipeline<TOptions>(string pipelineName, Action<TOptions> configure)
        where TOptions : RequestPipelineBuilderOptions;
}

public class HttpServer : IHttpServer
{
    public IServiceProvider Services { get; }
    public int Port => _tcpServer.Port;
    public Uri LocalEndpoint => _tcpServer.LocalEndpoint;
    
    private readonly TcpServer _tcpServer;
    private readonly RequestHandler _requestHandler;
    private readonly IPipelineRegistry _pipelineRegistry;

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
        var pipelineOptions = new RequestPipelineBuilderOptions(Services)
        {
            Name = $"Pipeline {Guid.NewGuid():N}"
        };
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }

    public IHttpServer AddPipeline<TOptions>(Action<TOptions> configure) where TOptions : RequestPipelineBuilderOptions
    {
        return AddPipeline(Guid.NewGuid().ToString("N"), configure);
    }

    public IHttpServer AddPipeline(string pipelineName, Action<RequestPipelineBuilderOptions> configure)
    {
        var pipelineOptions = new RequestPipelineBuilderOptions(Services)
        {
            Name = pipelineName
        };
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }
    
    public IHttpServer AddPipeline<TOptions>(string pipelineName, Action<TOptions> configure) where TOptions : RequestPipelineBuilderOptions
    {
        var pipelineOptions = ActivatorUtilities.CreateInstance<TOptions>(Services);
        pipelineOptions.Name = pipelineName;
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }
}