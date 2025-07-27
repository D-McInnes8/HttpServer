namespace HttpServer.Response;

/// <summary>
/// Represents a HTTP response.
/// </summary>
public enum HttpResponseStatusCode
{
    /// <summary>
    /// The request was successful.
    /// </summary>
    OK = 200,
    
    /// <summary>
    /// The resource is temporarily moved.
    /// </summary>
    Found = 302,
    
    /// <summary>
    /// The resource is permanently moved.
    /// </summary>
    MovePermanently = 301,
    
    /// <summary>
    /// The requested resource has not been modified since the last request.
    /// </summary>
    NotModified = 304,
    
    /// <summary>
    /// The request was not acceptable.
    /// </summary>
    BadRequest = 400,
    
    /// <summary>
    /// The request was not authorised.
    /// </summary>
    Unauthorized = 401,
    
    /// <summary>
    /// The request was forbidden.
    /// </summary>
    Forbidden = 403,
    
    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 404,
    
    /// <summary>
    /// The method used in the request is not allowed.
    /// </summary>
    MethodNotAllowed = 405,
    
    /// <summary>
    /// An unexpected error on the server occurred.
    /// </summary>
    InternalServerError = 500,
    
    /// <summary>
    /// The requested method or route is not implemented.
    /// </summary>
    NotImplemented = 501
}