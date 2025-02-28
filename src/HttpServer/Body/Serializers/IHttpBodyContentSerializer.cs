using System.Text;
using HttpServer.Headers;

namespace HttpServer.Body.Serializers;

public interface IHttpBodyContentSerializer
{
    public static abstract HttpBodyContent Deserialize(byte[] content, HttpContentType contentType, Encoding encoding);
}

public interface IHttpContentDeserializer
{
    /// <summary>
    /// Deserializes the byte array to a <see cref="HttpBodyContent"/>.
    /// </summary>
    /// <param name="content">The byte array to deserialize.</param>
    /// <param name="contentType">The content type of the http body content.</param>
    /// <param name="encoding">The encoding of the http body content.</param>
    /// <returns>The deserialized <see cref="HttpBodyContent"/>.</returns>
    public HttpBodyContent Deserialize(byte[] content, HttpContentType contentType, Encoding encoding);
}