using HttpServer.Pipeline;
using HttpServer.Pipeline.Registry;
using HttpServer.Request.Parser;
using HttpServer.Response.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace HttpServer;

/// <summary>
/// Represents the web server that listens for incoming HTTP requests.
/// </summary>
public interface IHttpWebServer
{
    /// <summary>
    /// The services available to the web server.
    /// </summary>
    IServiceProvider Services { get; }
    
    /// <summary>
    /// The port the web server is listening on.
    /// </summary>
    int Port { get; }
    
    /// <summary>
    /// The local endpoint the web server is listening on.
    /// </summary>
    Uri LocalEndpoint { get; }
    
    /// <summary>
    /// Starts the web server.
    /// </summary>
    /// <returns></returns>
    Task StartAsync();
    
    /// <summary>
    /// Stops the web server.
    /// </summary>
    /// <returns></returns>
    Task StopAsync();

    /// <summary>
    /// Adds a request pipeline to the web server.
    /// </summary>
    /// <param name="configure">Used to configure the <see cref="RequestPipelineBuilderOptions"/> for the pipeline.</param>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    public IHttpWebServer AddPipeline<TOptions>(Action<TOptions> configure) where TOptions : RequestPipelineBuilderOptions;
    
    /// <summary>
    /// Adds a request pipeline to the web server.
    /// </summary>
    /// <param name="pipelineName">The name of the request pipeline. The name must be unique.</param>
    /// <param name="configure">Used to configure the <see cref="RequestPipelineBuilderOptions"/> for the pipeline.</param>
    /// <typeparam name="TOptions"></typeparam>
    /// <returns></returns>
    IHttpWebServer AddPipeline<TOptions>(string pipelineName, Action<TOptions> configure)
        where TOptions : RequestPipelineBuilderOptions;
}

/// <summary>
/// The web server that listens for incoming HTTP requests.
/// </summary>
public class HttpWebWebServer : IHttpWebServer
{
    public IServiceProvider Services { get; }
    public int Port => _tcpServer.Port;
    public Uri LocalEndpoint => _tcpServer.LocalEndpoint;
    
    private readonly TcpServer _tcpServer;
    private readonly RequestHandler _requestHandler;
    private readonly IPipelineRegistry _pipelineRegistry;

    /// <summary>
    /// Creates a new instance of <see cref="HttpWebWebServer"/>. Should only
    /// be invoked internally by the <see cref="IHttpWebServerBuilder"/> object.
    /// </summary>
    /// <param name="port">The port the web server will listen on.</param>
    /// <param name="serviceProvider">The service provider for the web server.</param>
    internal HttpWebWebServer(int port, IServiceProvider serviceProvider)
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
    
    /// <summary>
    /// Creates a new instance of <see cref="IHttpWebServerBuilder"/> with the specified port.
    /// </summary>
    /// <param name="port">The port the web server will listen on.</param>
    /// <returns></returns>
    public static IHttpWebServerBuilder CreateBuilder(int port) => new HttpWebWebServerBuilder(port);
    
    public IHttpWebServer AddPipeline(Action<RequestPipelineBuilderOptions> configure)
    {
        var pipelineOptions = new RequestPipelineBuilderOptions(Services)
        {
            Name = $"Pipeline {Guid.NewGuid():N}"
        };
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }

    public IHttpWebServer AddPipeline<TOptions>(Action<TOptions> configure) where TOptions : RequestPipelineBuilderOptions
    {
        return AddPipeline(Guid.NewGuid().ToString("N"), configure);
    }

    public IHttpWebServer AddPipeline(string pipelineName, Action<RequestPipelineBuilderOptions> configure)
    {
        var pipelineOptions = new RequestPipelineBuilderOptions(Services)
        {
            Name = pipelineName
        };
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }
    
    public IHttpWebServer AddPipeline<TOptions>(string pipelineName, Action<TOptions> configure) where TOptions : RequestPipelineBuilderOptions
    {
        var pipelineOptions = ActivatorUtilities.CreateInstance<TOptions>(Services);
        pipelineOptions.Name = pipelineName;
        configure(pipelineOptions);
        _pipelineRegistry.AddPipeline(pipelineOptions.Name, pipelineOptions);
        return this;
    }
}