using Microsoft.Extensions.Logging;

namespace HttpServer.Logging;

/// <summary>
/// Represents a logger that writes log messages to a file.
/// </summary>
internal class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly FileLoggerWriter _logBuffer;

    /// <summary>
    /// Creates a new <see cref="FileLogger"/> with the specified category name and log buffer.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <param name="logBuffer"></param>
    public FileLogger(string categoryName, FileLoggerWriter logBuffer)
    {
        _categoryName = categoryName;
        _logBuffer = logBuffer;
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        
        var message = formatter(state, exception);
        _logBuffer.Enqueue($"[{logLevel}] [{_categoryName}] {message}");
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}