using HttpServer.Request;

namespace HttpServer.Routing;

/// <summary>
/// Routes HTTP requests to handlers.
/// </summary>
public interface IRouter
{
    /// <summary>
    /// Adds a route to the router registry.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="value"></param>
    public void AddRoute(Route path, RouteMetadata value);
    
    /// <summary>
    /// Attempts to match the provided path to a route.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public RouteMatch<RouteMetadata> Match(Route path);
}

/// <summary>
/// The default router implementation.
/// </summary>
internal class DefaultRouter : IRouter
{
    private readonly Dictionary<HttpRequestMethod, RoutingRadixTree<RouteMetadata>> _prefixTrees;

    /// <summary>
    /// Constructs a new <see cref="DefaultRouter"/>.
    /// </summary>
    public DefaultRouter()
    {
        _prefixTrees = new();
        foreach (var method in Enum.GetValues<HttpRequestMethod>())
        {
            _prefixTrees[method] = new RoutingRadixTree<RouteMetadata>();
        }
    }
    
    public void AddRoute(Route path, RouteMetadata value)
    {
        _prefixTrees[path.Method].AddRoute(path, value);
    }

    public RouteMatch<RouteMetadata> Match(Route path)
    {
        var result = _prefixTrees[path.Method].Match(path);
        if (result.Result == RouterResult.Success)
        {
            return result;
        }
        
        // Check for other methods with the same route and return a 405 if one is found.
        foreach (var method in Enum.GetValues<HttpRequestMethod>())
        {
            if (method == path.Method)
            {
                continue;
            }

            var match = _prefixTrees[method].Match(path);
            if (match.Result == RouterResult.Success)
            {
                return RouteMatch<RouteMetadata>.MethodNotAllowed;
            }
        }
        
        return RouteMatch<RouteMetadata>.NoMatch;
    }
}

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
}

public readonly record struct Option<TValue>
{
    /// <summary>
    /// 
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// 
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public Option(TValue value)
    {
        HasValue = true;
        Value = value;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Option<TValue>(TValue value) => new(value);
}

public static class Result
{
    public static Result<TValue, TError> Success<TValue, TError>(TValue value) => new(value);
    public static Result<TValue, TError> Error<TValue, TError>(TError error) => new(error);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TError"></typeparam>
public readonly record struct Result<TValue, TError>
{
    private readonly bool _success;
    private readonly TValue _value;
    private readonly TError _error;
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsSuccess => _success;
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsError => !_success;
    
    /// <summary>
    /// 
    /// </summary>
    public TValue Value => _value;
    
    /// <summary>
    /// 
    /// </summary>
    public TError Error => _error;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public Result(TValue value)
    {
        _success = true;
        _value = value;
        _error = default!;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    public Result(TError error)
    {
        _success = false;
        _value = default!;
        _error = error;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TError error) => new(error);
}