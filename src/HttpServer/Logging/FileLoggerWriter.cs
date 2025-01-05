using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace HttpServer.Logging;

/// <summary>
/// Represents a writer that writes log messages to a file.
/// </summary>
internal class FileLoggerWriter : IAsyncDisposable
{
    private readonly ConcurrentQueue<string> _buffer = new();
    private readonly string _filePath;

    private readonly Timer _flushTimer;
    private readonly Lock _writeLock;
    private readonly bool _flushImmediately;
    
    /// <summary>
    /// Creates a new <see cref="FileLoggerWriter"/> with the specified options.
    /// </summary>
    /// <param name="options"></param>
    public FileLoggerWriter(IOptionsMonitor<FileLoggerOptions> options)
    {
        var currentOptions = options.CurrentValue;
        _filePath = currentOptions.FilePath;
        _writeLock = new Lock();
        _flushImmediately = currentOptions.FlushImmediately;

        if (!currentOptions.AppendToExistingFile)
        {
            File.Delete(_filePath);
        }
        
        _flushTimer = new Timer(Flush, null, currentOptions.FlushInterval, currentOptions.FlushInterval);
    }
    
    /// <summary>
    /// Enqueues a message to be written to the log file.
    /// </summary>
    /// <param name="message"></param>
    public void Enqueue(string message)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        _buffer.Enqueue(message);
        if (_flushImmediately)
        {
            Flush();
        }
    }
    
    /// <summary>
    /// Flushes the buffer to the log file.
    /// </summary>
    /// <param name="obj"></param>
    private void Flush(object? obj = null)
    {
        lock (_writeLock)
        {
            if (_buffer.IsEmpty)
            {
                return;
            }
            
            using var writer = new StreamWriter(_filePath, append: true);
            while (_buffer.TryDequeue(out var log))
            {
                writer.WriteLine(log);
            }
            writer.Flush();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _flushTimer.DisposeAsync();
    }
}