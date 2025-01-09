using HttpServer.Request;
using Microsoft.Extensions.Logging;

namespace HttpServer.Routing.Internal;

/// <summary>
/// The default router implementation.
/// </summary>
internal class DefaultRouter : IRouter
{
    private readonly ILogger<DefaultRouter> _logger;
    private readonly Dictionary<HttpRequestMethod, RoutingRadixTree<RouteMetadata>> _prefixTrees;

    /// <summary>
    /// Constructs a new <see cref="DefaultRouter"/>.
    /// </summary>
    public DefaultRouter(ILogger<DefaultRouter> logger)
    {
        _logger = logger;
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
        _logger.LogDebug("Matched route '{Path}' with result '{Result}'", path.Path, result.Result);
        
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
                _logger.LogDebug("Route '{Path}' matched but method '{Method}' is not allowed", path.Path, path.Method);
                return RouteMatch<RouteMetadata>.MethodNotAllowed;
            }
        }
        
        return RouteMatch<RouteMetadata>.NoMatch;
    }
}