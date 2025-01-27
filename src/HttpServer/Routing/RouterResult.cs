namespace HttpServer.Routing;

/// <summary>
/// A result indicating the outcome of a router.
/// </summary>
public enum RouterResult
{
    /// <summary>
    /// The router was successful.
    /// </summary>
    Success,
    
    /// <summary>
    /// The router was not successful.
    /// </summary>
    NotFound,
    
    /// <summary>
    /// A path was found but the method was not allowed.
    /// </summary>
    MethodNotAllowed,
    
    /// <summary>
    /// The request sent wa an OPTIONS request.
    /// </summary>
    Options
}