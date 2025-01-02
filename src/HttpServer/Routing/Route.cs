using System.Diagnostics.CodeAnalysis;
using HttpServer.Request;

namespace HttpServer.Routing;

/// <summary>
/// 
/// </summary>
public readonly struct Route
{
    /// <summary>
    /// 
    /// </summary>
    public required string Path { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    public required HttpRequestMethod Method { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public Route()
    {
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="method"></param>
    [SetsRequiredMembers]
    public Route(string path, HttpRequestMethod method)
    {
        Path = path;
        Method = method;
    }
}