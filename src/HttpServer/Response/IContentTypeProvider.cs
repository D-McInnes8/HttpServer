namespace HttpServer.Response;

public interface IContentTypeProvider
{
    public HttpContentType GetContentType(string extension);
}