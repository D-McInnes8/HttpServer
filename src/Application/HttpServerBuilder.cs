using System.Diagnostics;
using Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public interface IHttpServerBuilder
{
    public IServiceCollection Services { get; }
    public int Port { get; }

    public HttpServer Build();
}

public class HttpServerBuilder : IHttpServerBuilder
{
    public IServiceCollection Services { get; }
    public int Port { get; }

    public HttpServerBuilder(int port)
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
    }

    public HttpServer Build()
    {
        Debug.Assert(Port >= 0);
        return new HttpServer(
            port: Port,
            serviceProvider: Services.BuildServiceProvider());
    }
}