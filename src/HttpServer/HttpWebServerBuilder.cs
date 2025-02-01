using System.Diagnostics;
using HttpServer.Logging;
using HttpServer.Pipeline.Registry;
using HttpServer.Routing;
using HttpServer.Routing.Internal;
using HttpServer.Routing.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    public IHttpWebServer Build();
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
        Port = port;
        
        Services = new ServiceCollection();
        AddRequiredServices();
    }

    private void AddRequiredServices()
    {
        Services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddFileLogger(options =>
            {
                options.FilePath = "http-server.log";
                options.FlushImmediately = true;
                options.AppendToExistingFile = false;
            });
        });
        Services.AddSingleton<IPipelineRegistry, DefaultPipelineRegistry>()
                .AddSingleton<IReadOnlyPipelineRegistry>(provider => provider.GetRequiredService<IPipelineRegistry>())
                .AddSingleton<IRouter, DefaultRouter>()
                .AddSingleton<HttpWebServerOptions>()
                .AddScoped<IFileContentTypeProvider, DefaultContentTypeProvider>();
    }
    
    /// <inheritdoc />
    public IHttpWebServer Build()
    {
        Debug.Assert(Port >= 0);
        return new HttpWebServer(
            port: Port,
            serviceProvider: Services.BuildServiceProvider());
    }
}