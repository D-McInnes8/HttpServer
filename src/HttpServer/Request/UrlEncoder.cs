namespace HttpServer.Request;

/// <summary>
/// Provides methods to encode and decode URLs.
/// </summary>
internal static class UrlEncoder
{
    /// <summary>
    /// Encodes a URL.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string Encode(string url)
    {
        return Uri.EscapeDataString(url);
    }
    
    /// <summary>
    /// Decodes a URL.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string Decode(string url)
    {
        var unescapedUrl = Uri.UnescapeDataString(url);
        var result = new char[unescapedUrl.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = unescapedUrl[i] switch
            {
                '+' => ' ',
                _ => unescapedUrl[i]
            };
        }
        
        return new string(result);
    }
}