using System.Text;
using HttpServer.Headers;

namespace HttpServer.Body.Serializers;

/// <summary>
/// Represents a serializer for <see cref="StringBodyContent"/>.
/// </summary>
public class StringBodyContentSerializer : IHttpContentDeserializer
{
    public HttpBodyContent Deserialize(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        return new StringBodyContent(encoding.GetString(content), contentType, encoding);
    }
}