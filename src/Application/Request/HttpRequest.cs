namespace Application.Request;

public class HttpRequest
{
    public required HttpRequestMethod Method { get; init; }
    public required string Path { get; init; }
    public required bool HasBody { get; init; }
}