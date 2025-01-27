namespace HttpServer.Routing;

/// <summary>
/// The result indicating the outcome of matching a route in the router.
/// </summary>
/// <typeparam name="T">The type used to store metadata for each route.</typeparam>
public class RouteMatch<T>
{
    /// <summary>
    /// The <see cref="RouterResult"/> indicating result of the route match.
    /// </summary>
    public RouterResult Result { get; set; }
    
    /// <summary>
    /// The value associated with the route match.
    /// </summary>
    public T? Value { get; set; }
    
    /// <summary>
    /// The parameters extracted from the route.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
    
    /// <summary>
    /// The allowed methods for the route.
    /// </summary>
    public string[] AllowedMethods { get; set; } = [];

    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> using the provided result and value.
    /// </summary>
    /// <param name="result">The result of the route match.</param>
    /// <param name="value">The value to associate with the route match.</param>
    private RouteMatch(RouterResult result, T? value)
    {
        Result = result;
        Value = value;
    }

    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> using the provided result, value, and parameters.
    /// </summary>
    /// <param name="result">The result of the route match.</param>
    /// <param name="value">The value to associate with the route match.</param>
    /// <param name="parameters">The parameters extracted from the route.</param>
    private RouteMatch(RouterResult result, T? value, Dictionary<string, string> parameters)
    {
        Result = result;
        Value = value;
        Parameters = parameters;
    }

    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> using the provided allowed methods.
    /// The result will be set to <see cref="RouterResult.Options"/>.
    /// </summary>
    /// <param name="allowedMethods">The HTTP methods allowed for this route.</param>
    private RouteMatch(string[] allowedMethods)
    {
        Result = RouterResult.Options;
        AllowedMethods = allowedMethods;
        Value = default;
    }
    
    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> with a NotFound result using the provided value.
    /// </summary>
    public static RouteMatch<T> NoMatch => new RouteMatch<T>(RouterResult.NotFound, default);
    
    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> with a Success result using the provided value.
    /// </summary>
    /// <param name="value">The value to associate with the route match.</param>
    /// <returns>The constructed <see cref="RouteMatch{T}"/> object.</returns>
    public static RouteMatch<T> Match(T? value) => new RouteMatch<T>(RouterResult.Success, value);
    
    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> with a Success result using the provided value and parameters.
    /// </summary>
    /// <param name="value">The value to associate with the route match.</param>
    /// <param name="parameters">The parameters extracted from the route.</param>
    /// <returns>The constructed <see cref="RouteMatch{T}"/> object.</returns>
    public static RouteMatch<T> Match(T? value, Dictionary<string, string> parameters) => new RouteMatch<T>(RouterResult.Success, value, parameters);
    
    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> with a MethodNotAllowed result.
    /// </summary>
    public static RouteMatch<T> MethodNotAllowed => new RouteMatch<T>(RouterResult.MethodNotAllowed, default);
    
    /// <summary>
    /// Constructs a new <see cref="RouteMatch{T}"/> with a Options result using the provided allowed methods.
    /// </summary>
    /// <param name="allowedMethods">The HTTP methods allowed for this route.</param>
    /// <returns>The constructed <see cref="RouteMatch{T}"/> object.</returns>
    public static RouteMatch<T> Options(string[] allowedMethods) => new (allowedMethods);
}