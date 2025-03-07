using HttpServer.Networking;
using HttpServer.Pipeline;
using HttpServer.Pipeline.Registry;
using HttpServer.Request;
using HttpServer.Request.Parser;
using HttpServer.Response;
using HttpServer.Response.Internal;
using HttpServer.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    /// The options for the web server.
    /// </summary>
    HttpWebServerOptions Options { get; }
    
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

    public IHttpWebServer AddPipeline(Action<RequestPipelineBuilderOptions> configure);
    public IHttpWebServer AddPipeline(string pipelineName, Action<RequestPipelineBuilderOptions> configure);
    
    public IHttpWebServer MapRoute(HttpRequestMethod method, string path, RouteMetadata metadata);
    public IHttpWebServer MapRoute(HttpRequestMethod method, string path, Func<RequestPipelineContext, HttpResponse> handler);
    public IHttpWebServer MapRoute(HttpRequestMethod method, string path, string pipelineName, Func<RequestPipelineContext, HttpResponse> handler);
}

/// <summary>
/// The web server that listens for incoming HTTP requests.
/// </summary>
public class HttpWebServer : IHttpWebServer
{
    public IServiceProvider Services { get; }
    public int Port => _tcpServer.Port;
    public Uri LocalEndpoint => _tcpServer.LocalEndpoint;
    public HttpWebServerOptions Options => _options;
    
    private readonly TcpServer _tcpServer;
    private readonly IPipelineRegistry _pipelineRegistry;
    private readonly IRouter _router;
    private readonly ILogger<HttpWebServer> _logger;
    private readonly HttpWebServerOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="HttpWebServer"/>. Should only
    /// be invoked internally by the <see cref="IHttpWebServerBuilder"/> object.
    /// </summary>
    /// <param name="port">The port the web server will listen on.</param>
    /// <param name="serviceProvider">The service provider for the web server.</param>
    internal HttpWebServer(int port, IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        
        ArgumentOutOfRangeException.ThrowIfNegative(port, nameof(port));
        _options = serviceProvider.GetRequiredService<HttpWebServerOptions>();
        _tcpServer = new TcpServer(port, HandleRequest, loggerFactory.CreateLogger<TcpServer>(), serviceProvider.GetRequiredService<IConnectionPool>(), _options);
        _pipelineRegistry = serviceProvider.GetRequiredService<IPipelineRegistry>();
        _router = serviceProvider.GetRequiredService<IRouter>();
        _logger = loggerFactory.CreateLogger<HttpWebServer>();
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Starting web server");
        await _tcpServer.StartAsync();
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping web server");
        await _tcpServer.StopAsync();
    }

    private HttpResponse HandleRequest(INetworkStreamReader streamReader)
    {
        using var scope = Services.CreateScope();
        var httpRequestParser = scope.ServiceProvider.GetRequiredService<HttpRequestParser>();

        HttpRequest? httpRequest;
        try
        {
            var result = httpRequestParser.Parse(streamReader).GetAwaiter().GetResult();
            httpRequest = result.Value;
        }
        catch (HttpParserException ex) when (!ex.InternalServerError)
        {
            return ex.ResponseMessage is not null 
                ? HttpResponse.BadRequest(ex.ResponseMessage) 
                : HttpResponse.BadRequest();
        }

        return ExecuteRequestPipeline(httpRequest, scope);
    }

    private HttpResponse ExecuteRequestPipeline(HttpRequest httpRequest, IServiceScope scope)
    {
        var ctx = new RequestPipelineContext(httpRequest, scope.ServiceProvider, _pipelineRegistry.GlobalPipeline.Options);
        var logState = new List<KeyValuePair<string, object?>>
        {
            new("RequestId", ctx.RequestId),
        };
        using (_logger.BeginScope(logState))
        {
            _logger.LogInformation("Received request: {Method} {Path}", httpRequest.Method, httpRequest.Path);
            var httpResponse = _pipelineRegistry.GlobalPipeline.ExecuteAsync(ctx).GetAwaiter().GetResult();
            
            _logger.LogInformation("Sending response: {StatusCode}", httpResponse.StatusCode);
            return httpResponse;
        }
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
    
    public IHttpWebServer MapRoute(HttpRequestMethod method, string path, RouteMetadata metadata)
    {
        _router.AddRoute(new Route(path, method), metadata);
        return this;
    }

    public IHttpWebServer MapRoute(HttpRequestMethod method, string path, Func<RequestPipelineContext, HttpResponse> handler)
    {
        var metadata = new RouteMetadata
        {
            Handler = handler,
        };
        _router.AddRoute(new Route(path, method), metadata);
        return this;
    }

    public IHttpWebServer MapRoute(HttpRequestMethod method, string path, string pipelineName, Func<RequestPipelineContext, HttpResponse> handler)
    {
        var metadata = new RouteMetadata
        {
            Handler = handler,
            Pipeline = pipelineName,
        };
        _router.AddRoute(new Route(path, method), metadata);
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