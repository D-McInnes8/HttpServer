using System.Diagnostics.CodeAnalysis;

namespace HttpServer.Request.Parser;

/// <summary>
/// Represents an exception thrown when an error occurs while parsing an HTTP request.
/// </summary>
public class HttpParserException : Exception
{
    /// <summary>
    /// Gets the response message to be sent to the client.
    /// </summary>
    public string? ResponseMessage { get; }
    
    /// <summary>
    /// Gets or sets a value indicating whether an internal server error occurred.
    /// </summary>
    public bool InternalServerError { get; set; }
    
    /// <summary>
    /// Gets or sets the error code of the exception.
    /// </summary>
    public HttpParserExceptionErrorCode ErrorCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpParserException"/> class with the specified error code.
    /// </summary>
    /// <param name="errorCode">The error code of the exception.</param>
    public HttpParserException(HttpParserExceptionErrorCode errorCode)
    {
        ErrorCode = errorCode;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpParserException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public HttpParserException(string message) : base(message)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpParserException"/>
    /// class with the specified message and response message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="responseMessage">The response message to be sent to the client.</param>
    public HttpParserException(string message, string responseMessage) : base(message)
    {
        ResponseMessage = responseMessage;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpParserException"/> class with the specified message,
    /// response message, and flag the exception as being caused by an internal server error.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="responseMessage">The response message to be sent to the client.</param>
    /// <param name="internalServerError">A value indicating whether an internal server error occurred.</param>
    public HttpParserException(string message, string responseMessage, bool internalServerError) : base(message)
    {
        ResponseMessage = responseMessage;
        InternalServerError = internalServerError;
    }

    /// <summary>
    /// Throws an <see cref="HttpParserException"/> if the specified value is null.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="errorCode">The error code of the exception.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <exception cref="HttpParserException">The value is null.</exception>
    public static void ThrowIfNull<T>([NotNull] T? value, HttpParserExceptionErrorCode errorCode)
    {
        if (value is null)
        {
            throw new HttpParserException(errorCode);
        }
    }
    
    /// <summary>
    /// Throws an <see cref="HttpParserException"/> if the specified value is null or empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="errorCode">The error code of the exception.</param>
    /// <exception cref="HttpParserException">The value is null or empty.</exception>
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? value, HttpParserExceptionErrorCode errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new HttpParserException(errorCode);
        }
    }
}

public enum HttpParserExceptionErrorCode
{
    InvalidRequestLine = 1,
    InvalidHttpVersion = 2,
    InvalidMethod = 3,
    InvalidUri = 4,
    InvalidHeader = 5,
    InvalidContentLength = 6,
    InvalidContentType = 7,
    InvalidTransferEncoding = 8,
    InvalidChunkSize = 9,
    InvalidChunkExtension = 10,
    InvalidChunkData = 11,
    InvalidMultipartBoundary = 12,
    InvalidMultipartEnd = 13,
}

/// <summary>
/// Represents an exception thrown when an error occurs while parsing a multipart request.
/// </summary>
public class MultipartParserException : HttpParserException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipartParserException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MultipartParserException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipartParserException"/> class with the specified message and response message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="responseMessage">The response message to be sent to the client.</param>
    public MultipartParserException(string message, string responseMessage) : base(message, responseMessage)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipartParserException"/> class with the specified message,
    /// response message, and flag the exception as being caused by an internal server error.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="responseMessage">The response message to be sent to the client.</param>
    /// <param name="internalServerError">A value indicating whether an internal server error occurred.</param>
    public MultipartParserException(string message, string responseMessage, bool internalServerError) : base(message, responseMessage, internalServerError)
    {
    }
}