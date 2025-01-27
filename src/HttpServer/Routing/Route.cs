using System.Diagnostics.CodeAnalysis;
using HttpServer.Request;

namespace HttpServer.Routing;

/// <summary>
/// Represents a route in the router.
/// </summary>
public readonly struct Route
{
    /// <summary>
    /// The path of the route.
    /// </summary>
    public required string Path { get; init; }
    
    /// <summary>
    /// The HTTP method of the route.
    /// </summary>
    public required HttpRequestMethod Method { get; init; }
    
    /// <summary>
    /// Constructs a new <see cref="Route"/> with the provided path and method.
    /// </summary>
    /// <param name="path">The path of the route.</param>
    /// <param name="method">The HTTP method of the route.</param>
    [SetsRequiredMembers]
    public Route(string path, HttpRequestMethod method)
    {
        Path = path;
        Method = method;
    }
}