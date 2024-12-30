using System.Diagnostics.CodeAnalysis;
using Application.Request;

namespace Application.Pipeline;

/// <summary>
/// The context for the request pipeline.
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
    /// Construct a new <see cref="RequestPipelineContext"/> for the provided <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="request">The HTTP request associated with this context.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> scoped to this request.</param>
    /// <exception cref="ArgumentNullException">
    /// If either the <paramref name="request"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    [SetsRequiredMembers]
    public RequestPipelineContext(HttpRequest request, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        Request = request;
        Services = serviceProvider;
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
}

/// <summary>
/// Represents a data object that can be stored within a <see cref="RequestPipelineContext"/>.
/// </summary>
public interface IPipelineData
{
    
}