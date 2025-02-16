using Microsoft.Extensions.Logging;

namespace HttpServer.Logging;

/// <summary>
/// Represents a logger that writes log messages to a file.
/// </summary>
internal class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly FileLoggerWriter _logBuffer;
    private readonly IExternalScopeProvider? _scopeProvider;

    /// <summary>
    /// Creates a new <see cref="FileLogger"/> with the specified category name and log buffer.
    /// </summary>
    /// <param name="categoryName">The category name of the logger.</param>
    /// <param name="logBuffer">The log buffer to write log messages to.</param>
    /// <param name="scopeProvider">The external scope provider to provide scope information.</param>
    public FileLogger(string categoryName, FileLoggerWriter logBuffer, IExternalScopeProvider? scopeProvider)
    {
        _categoryName = categoryName;
        _logBuffer = logBuffer;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string? requestId = null;
        _scopeProvider?.ForEachScope((scope, scopeState) =>
        {
            if (scope is IEnumerable<KeyValuePair<string, object>> stateDictionary)
            {
                foreach (var (key, value) in stateDictionary)
                {
                    if (key == "RequestId")
                    {
                        requestId = value as string;
                    }
                }
            }
        }, state);
        
        var message = formatter(state, exception);
        _logBuffer.Enqueue(requestId is not null
            ? $"[{logLevel}] [{_categoryName}] [{requestId}] {message}"
            : $"[{logLevel}] [{_categoryName}] {message}");
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _scopeProvider?.Push(state);
}