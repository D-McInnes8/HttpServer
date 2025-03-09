using System.Collections.Specialized;
using System.Text;
using HttpServer.Headers;
using HttpServer.Request.Parser;

namespace HttpServer.Body.Serializers;

/// <summary>
/// Represents a serializer for <see cref="MultipartFormDataBodyContent"/>.
/// </summary>
public class MultipartFormDataBodyContentSerializer : IHttpContentDeserializer
{
    private readonly IHttpBodyContentSerializerProvider _serializerProvider;

    public MultipartFormDataBodyContentSerializer(IHttpBodyContentSerializerProvider serializerProvider)
    {
        _serializerProvider = serializerProvider;
    }

    public HttpBodyContent Deserialize(byte[] content, HttpContentType contentType, Encoding encoding)
    {
        HttpParserException.ThrowIfNull(contentType.Boundary, HttpParserExceptionErrorCode.InvalidMultipartBoundary);
        //var boundary = contentType.Boundary;
        /*return MultipartFormDataBodyContent.Parse(content, contentType, encoding);*/
        
        //ReadOnlySpan<byte> span = content.AsSpan();
        
        var result = new MultipartFormDataBodyContent(content, contentType, encoding);
        var reader = new MultipartContentReader(content, encoding, contentType.Boundary);
        while (!reader.IsFinalBoundary())
        {
            var partContent = reader.ReadToNextBoundary();
            if (partContent.IsEmpty)
            {
                continue;
            }
            
            var partReader = new MultipartContentPartReader(partContent, System.Text.Encoding.ASCII);

            var headers = new NameValueCollection();
            while (partReader.HasRemainingHeaders())
            {
                var header = partReader.ReadNextHeader();
                if (HttpRequestParser.TryGetParsedHeader(header, out var parsedHeader))
                {
                    headers.Add(parsedHeader.Key, parsedHeader.Value);
                }
            }

            var httpContentType = HttpContentType.ApplicationOctetStream;
            if (headers["Content-Type"] is not null
                && HttpContentType.TryParse(headers["Content-Type"], out var partContentType))
            {
                httpContentType = partContentType;
            }
            
            ContentDisposition? contentDisposition = null;
            if (headers["Content-Disposition"] is not null
                && ContentDisposition.TryParse(headers["Content-Disposition"], null, out var partDisposition))
            {
                contentDisposition = partDisposition;
            }
            
            var partBody = partReader.ReadToEnd();
            var partEncoding = httpContentType.Charset is not null ? Encoding.GetEncoding(httpContentType.Charset) : Encoding.ASCII;
            var deserializer = _serializerProvider.GetSerializer(httpContentType);
            var partBodyContent = deserializer.Deserialize(partBody.ToArray(), httpContentType, partEncoding);
            partBodyContent.ContentDisposition = contentDisposition;
            result.Add(partBodyContent);
        }
        
        return result;
    }
}