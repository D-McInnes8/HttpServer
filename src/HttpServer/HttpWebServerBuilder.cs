using System.Diagnostics;
using HttpServer.Pipeline.Registry;
using HttpServer.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HttpServer;

/// <summary>
/// Represents a builder for creating an <see cref="HttpWebWebServer"/>.
/// </summary>
public interface IHttpWebServerBuilder
{
    /// <summary>
    /// The service collection used to build the web server.
    /// </summary>
    public IServiceCollection Services { get; }
    
    /// <summary>
    /// The port the web server will listen on.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Builds the <see cref="HttpWebWebServer"/>.
    /// </summary>
    /// <returns>The constructed <see cref="IHttpWebServer"/>.</returns>
    public HttpWebWebServer Build();
}

/// <summary>
/// A builder for creating an <see cref="HttpWebWebServer"/>.
/// </summary>
public class HttpWebWebServerBuilder : IHttpWebServerBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }
    
    /// <inheritdoc />
    public int Port { get; }

    /// <summary>
    /// Creates a new <see cref="HttpWebWebServerBuilder"/> with the specified port.
    /// </summary>
    /// <param name="port">The port the web server will listen on.</param>
    public HttpWebWebServerBuilder(int port)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(port, nameof(port));
        Services = new ServiceCollection();
        Port = port;
        
        AddRequiredServices();
    }

    private void AddRequiredServices()
    {
        Services.AddSingleton<IRouteRegistry, RouteRegistry>();
        Services.AddScoped<RoutingPlugin>();

        Services.AddSingleton<IPipelineRegistry, DefaultPipelineRegistry>()
                .AddSingleton<IReadOnlyPipelineRegistry>(provider => provider.GetRequiredService<IPipelineRegistry>());
    }
    
    /// <inheritdoc />
    public HttpWebWebServer Build()
    {
        Debug.Assert(Port >= 0);
        return new HttpWebWebServer(
            port: Port,
            serviceProvider: Services.BuildServiceProvider());
    }
}