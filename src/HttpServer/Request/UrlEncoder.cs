namespace HttpServer.Request;

internal static class UrlEncoder
{
    public static string Encode(string url)
    {
        return Uri.EscapeDataString(url);
    }
    
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