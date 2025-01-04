using System.Diagnostics.CodeAnalysis;
using HttpServer.Request;
using HttpServer.Routing;

namespace HttpServer.Pipeline;

/// <summary>
/// Represents a data object that can be stored within a <see cref="RequestPipelineContext"/>.
/// </summary>
public interface IPipelineData
{
}

/// <summary>
/// The context for a request pipeline. This context is used to store data objects that are shared between
/// different plugins in the pipeline. A single context may be used in multiple pipelines, if the server is configured
/// to use nested request pipelines.
/// </summary>
public class RequestPipelineContext
{
    private readonly Dictionary<Type, object> _data = new();
    
    /// <summary>
    /// The HTTP request associated with this context.
    /// </summary>
    public required HttpRequest Request { get; init; }
    
    /// <summary>
    /// The service provider associated with this context.
    /// </summary>
    public required IServiceProvider Services { get; init; }
    
    /// <summary>
    /// The <see cref="RequestPipelineBuilderOptions"/> options associated with this context.
    /// </summary>
    public RequestPipelineBuilderOptions Options { get; internal set; }
    
    /// <summary>
    /// The route metadata associated with this context. This property will only be populated after the
    /// global request pipeline has executed the router and matched a route.
    /// </summary>
    public RouteMetadata? Route { get; internal set; }

    /// <summary>
    /// Construct a new <see cref="RequestPipelineContext"/> for the provided <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="request">The HTTP request associated with this context.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> scoped to this request.</param>
    /// <param name="options">The <see cref="RequestPipelineBuilderOptions"/> associated with the current pipeline.</param>
    /// <exception cref="ArgumentNullException">
    /// If either the <paramref name="request"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    [SetsRequiredMembers]
    public RequestPipelineContext(HttpRequest request, IServiceProvider serviceProvider, RequestPipelineBuilderOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        Request = request;
        Services = serviceProvider;
        Options = options;
    }
    
    /// <summary>
    /// Get the data of the specified type from the context.
    /// </summary>
    /// <typeparam name="TData">The type of the data object being accessed.</typeparam>
    /// <returns>The data object registered for this type, otherwise <see langword="null"/></returns>
    public TData? GetData<TData>() where TData : IPipelineData
    {
        if (_data.TryGetValue(typeof(TData), out var data))
        {
            return (TData)data;
        }

        return default;
    }
    
    /// <summary>
    /// Set the data of the specified type in the context.
    /// </summary>
    /// <param name="data">The data object to be stored within the context.</param>
    /// <typeparam name="TData">The type of the data object being stored.</typeparam>
    public void SetData<TData>(TData data) where TData : IPipelineData
    {
        _data[typeof(TData)] = data;
    }
    
    /// <summary>
    /// Can be used to retrieve the options of the specified type from the context.
    /// </summary>
    /// <typeparam name="TOptions">The derived <see cref="RequestPipelineBuilderOptions"/> type to retrieve.</typeparam>
    /// <returns>The <typeparamref name="TOptions"/> if the pipeline options are of that type, otherwise null.</returns>
    public TOptions? GetOptions<TOptions>() where TOptions : RequestPipelineBuilderOptions
    {
        return Options as TOptions;
    }
}