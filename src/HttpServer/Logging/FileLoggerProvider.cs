using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HttpServer.Logging;

/// <summary>
/// Represents a provider for creating <see cref="FileLogger"/> instances.
/// </summary>
internal class FileLoggerProvider : ILoggerProvider
{
    private readonly IOptionsMonitor<FileLoggerOptions> _options;
    private readonly FileLoggerWriter _logBuffer;

    /// <summary>
    /// Creates a new <see cref="FileLoggerProvider"/> with the specified options.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logBuffer"></param>
    public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options, FileLoggerWriter logBuffer)
    {
        _options = options;
        _logBuffer = logBuffer;
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logBuffer);
    }
}