using HttpServer.Headers;

namespace HttpServer.Routing.StaticFiles;

/// <summary>
/// Provides content types for files based on their extension.
/// </summary>
public interface IFileContentTypeProvider
{
    /// <summary>
    /// Gets the content type for the specified file extension.
    /// Will default to <see cref="HttpContentType.TextPlain"/> if no content type is found.
    /// </summary>
    /// <param name="extension">The extension of the file.</param>
    /// <returns>The <see cref="HttpContentType"/> to be returned in the response.</returns>
    public HttpContentType GetContentType(string extension);
}