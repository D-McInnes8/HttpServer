namespace HttpServer.Routing;

/// <summary>
/// Represents the parameters of a route.
/// </summary>
public class RouteParameters : Dictionary<string, string>
{
    /// <summary>
    /// Constructs a new <see cref="RouteParameters"/> object.
    /// </summary>
    public RouteParameters()
    {
    }
    
    /// <summary>
    /// Constructs a new <see cref="RouteParameters"/> object from the provided dictionary.
    /// </summary>
    /// <param name="dictionary"></param>
    public RouteParameters(IDictionary<string, string> dictionary) : base(dictionary)
    {
    }
    
    /// <summary>
    /// Gets the wildcard parameter.
    /// </summary>
    public string Wildcard => this["*"];
    
    /// <summary>
    /// Tries to get the wildcard parameter.
    /// </summary>
    /// <param name="value">The wildcard parameter if one is set.</param>
    /// <returns>True if the wildcard parameter has been set, otherwise false.</returns>
    public bool TryGetWildcard(out string? value) => TryGetValue("*", out value);
}