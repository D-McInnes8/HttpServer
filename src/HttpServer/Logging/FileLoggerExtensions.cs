using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace HttpServer.Logging;

/// <summary>
/// Contains extension methods for configuring the file logger.
/// </summary>
public static class FileLoggerExtensions
{
    /// <summary>
    /// Adds a file logger to the logging system.
    /// </summary>
    /// <param name="builder">The logging builder to add the file logger to.</param>
    /// <returns>The logging builder with the file logger added.</returns>
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.AddSingleton<FileLoggerWriter>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(builder.Services);
        return builder;
    }
    
    /// <summary>
    /// Adds a file logger to the logging system.
    /// </summary>
    /// <param name="builder">The logging builder to add the file logger to.</param>
    /// <param name="configure">Used to configure the file logger options.</param>
    /// <returns>The logging builder with the file logger added.</returns>
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
    {
        builder.AddFileLogger();
        builder.Services.Configure(configure);
        return builder;
    }
}