namespace HttpServer.Plugins.ResponseCaching;

public class CacheControlHeader
{
    /// <summary>
    /// Indicates that the response will not change over time and can be cached indefinitely.
    /// </summary>
    public bool Immutable { get; init; }
    
    /// <summary>
    /// Indicates that any cache should not cache the response.
    /// </summary>
    public bool NoCache { get; init; }
    
    /// <summary>
    /// Indicates that the response should not be stored in any cache.
    /// </summary>
    public bool NoStore { get; init; }
    
    /// <summary>
    /// The maximum amount of time the response is considered fresh.
    /// </summary>
    public TimeSpan? MaxAge { get; init; }
    
    /// <summary>
    /// The maximum amount of time the response can be stale before it must be revalidated.
    /// </summary>
    public TimeSpan? MaxStale { get; init; }
    
    /// <summary>
    /// The minimum amount of time the response is considered fresh.
    /// </summary>
    public TimeSpan? MinFresh { get; init; }
    
    /// <summary>
    /// Indicates that the response must be revalidated before it can be used.
    /// </summary>
    public bool MustRevalidate { get; init; }
    
    /// <summary>
    /// Indicates that the response should not be transformed by any intermediary caches.
    /// </summary>
    public bool NoTransform { get; init; }
    
    /// <summary>
    /// Indicates that the response should only be served from cache and not revalidated with the origin server.
    /// </summary>
    public bool OnlyIfCached { get; init; }
    
    /// <summary>
    /// Indicates that the response should be revalidated with the origin server before being served from cache.
    /// </summary>
    public bool ProxyRevalidate { get; init; }
    
    /// <summary>
    /// Indicates that the response is public and can be cached by any cache, even if it is normally non-cacheable.
    /// </summary>
    public bool Public { get; init; }
    
    /// <summary>
    /// Indicates that the response is private and should not be cached by shared caches.
    /// </summary>
    public bool Private { get; init; }
    
    /// <summary>
    /// The shared max-age directive indicates that the response can be cached by shared caches (e.g., proxies) for a specified amount of time.
    /// </summary>
    public bool SharedMaxAge { get; init; }
}