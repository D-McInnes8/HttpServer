using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HttpServer.Plugins.ResponseCompression.DependencyInjection;

/// <summary>
/// Extension methods for adding response compression services to the DI container.
/// </summary>
public static class ResponseCompressionServiceCollectionExtensions
{
    /// <summary>
    /// Adds response compression services to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining calls.</returns>
    public static IServiceCollection AddResponseCompression(this IServiceCollection services)
    {
        services.TryAddSingleton<ICompressionProvider, GZipCompressionProvider>();
        services.TryAddSingleton<ICompressionProvider, DeflateCompressionProvider>();
        services.TryAddSingleton<ICompressionProvider, BrotliCompressionProvider>();
        return services;
    }
}