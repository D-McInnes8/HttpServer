namespace HttpServer.Logging;

/// <summary>
/// Represents the options for configuring a <see cref="FileLogger"/>.
/// </summary>
public class FileLoggerOptions
{
    /// <summary>
    /// The path to the file to write log messages to.
    /// </summary>
    public required string FilePath { get; set; }
    
    /// <summary>
    /// The interval at which to flush the log buffer to the file.
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Whether to flush the log buffer immediately after writing a message.
    /// </summary>
    public bool FlushImmediately { get; set; } = false;
    
    /// <summary>
    /// Whether to append to an existing log file or overwrite it.
    /// </summary>
    public bool AppendToExistingFile { get; set; } = true;
}