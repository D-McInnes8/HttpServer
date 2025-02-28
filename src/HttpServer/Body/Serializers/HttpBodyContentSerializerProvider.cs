using HttpServer.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace HttpServer.Body.Serializers;

/// <summary>
/// Provider to get the correct <see cref="IHttpContentDeserializer"/> for a given <see cref="HttpContentType"/>.
/// </summary>
public interface IHttpBodyContentSerializerProvider
{
    /// <summary>
    /// Adds a serializer for the given <see cref="HttpContentType"/>.
    /// </summary>
    /// <param name="contentType">The content type to add the serializer for.</param>
    /// <typeparam name="TDeserializer">The type of the serializer.</typeparam>
    public void AddSerializer<TDeserializer>(HttpContentType contentType) where TDeserializer : IHttpContentDeserializer;

    /// <summary>
    /// Adds a serializer for the given <see cref="HttpContentType"/>.
    /// </summary>
    /// <param name="contentTypes">The content types to add the serializer for.</param>
    /// <typeparam name="TDeserializer">The type of the serializer.</typeparam>
    public void AddSerializer<TDeserializer>(IEnumerable<HttpContentType> contentTypes)
        where TDeserializer : IHttpContentDeserializer;

    /// <summary>
    /// Gets the correct <see cref="IHttpContentDeserializer"/> for the given <see cref="HttpContentType"/>.
    /// </summary>
    /// <param name="contentType">The content type to get the serializer for.</param>
    /// <returns>The correct <see cref="IHttpContentDeserializer"/> for the given <see cref="HttpContentType"/>.</returns>
    public IHttpContentDeserializer GetSerializer(HttpContentType contentType);
}

/// <summary>
/// Default implementation of <see cref="IHttpBodyContentSerializerProvider"/>.
/// </summary>
internal class HttpBodyContentSerializerProvider : IHttpBodyContentSerializerProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<HttpContentType, Type> _deserializers;

    /// <summary>
    /// Constructs a new <see cref="HttpBodyContentSerializerProvider"/>.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public HttpBodyContentSerializerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _deserializers = new Dictionary<HttpContentType, Type>();
        AddSerializer<StringBodyContentSerializer>(HttpContentType.TextPlain);
    }
    
    public void AddSerializer<TDeserializer>(HttpContentType contentType) where TDeserializer : IHttpContentDeserializer
    {
        _deserializers[contentType] = typeof(TDeserializer);
    }
    
    public void AddSerializer<TDeserializer>(IEnumerable<HttpContentType> contentTypes) where TDeserializer : IHttpContentDeserializer
    {
        foreach (var contentType in contentTypes)
        {
            AddSerializer<TDeserializer>(contentType);
        }
    }

    public IHttpContentDeserializer GetSerializer(HttpContentType contentType)
    {
        if (_deserializers.TryGetValue(contentType, out var serializer))
        {
            if (ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, serializer)
                is IHttpContentDeserializer httpBodyContentSerializer)
            {
                return httpBodyContentSerializer;
            }
            
            throw new InvalidOperationException($"Invalid serializer found for content type '{contentType}'");
        }

        return new ByteArrayBodyContentSerializer();
    }
}