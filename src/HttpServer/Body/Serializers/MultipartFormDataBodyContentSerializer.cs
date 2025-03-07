using System.Text;
using HttpServer.Headers;

namespace HttpServer.Body.Serializers;

/// <summary>
/// Represents a serializer for <see cref="MultipartFormDataBodyContent"/>.
/// </summary>
public class MultipartFormDataBodyContentSerializer : IHttpContentDeserializer
{
    public HttpBodyContent Deserialize(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        var boundary = contentType.Boundary ?? throw new InvalidOperationException("Missing boundary");
        return MultipartFormDataBodyContent.Parse(content, contentType, encoding);
    }
}