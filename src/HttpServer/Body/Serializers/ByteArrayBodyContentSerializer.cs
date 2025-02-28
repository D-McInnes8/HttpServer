using System.Text;
using HttpServer.Headers;

namespace HttpServer.Body.Serializers;

/// <summary>
/// Represents a serializer for <see cref="ByteArrayBodyContent"/>.
/// </summary>
public class ByteArrayBodyContentSerializer : IHttpContentDeserializer
{
    public HttpBodyContent Deserialize(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        return new ByteArrayBodyContent(content, contentType, encoding);
    }
}