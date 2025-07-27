using System.Diagnostics;
using System.Text;
using HttpServer.Body;
using HttpServer.Headers;

namespace HttpServer.Plugins.ResponseCompression;

public class CompressedBodyContent : HttpBodyContent
{
    public HttpContentType ContentType => InnerContent.ContentType;
    public Encoding Encoding => InnerContent.Encoding;
    public byte[] Content => InnerContent.Content;
    public int Length => InnerContent.Length;

    public ContentDisposition? ContentDisposition
    {
        get => InnerContent.ContentDisposition;
        set => InnerContent.ContentDisposition = value;
    }
    
    /// <summary>
    /// The inner content that is being compressed.
    /// </summary>
    public HttpBodyContent InnerContent { get; }
    
    /// <summary>
    /// The compression provider used to compress the body content.
    /// </summary>
    public ICompressionProvider CompressionProvider { get; }
    
    public CompressedBodyContent(HttpBodyContent innerContent, ICompressionProvider compressionProvider)
    {
        ArgumentNullException.ThrowIfNull(innerContent);
        ArgumentNullException.ThrowIfNull(compressionProvider);

        InnerContent = innerContent;
        CompressionProvider = compressionProvider;
    }
    
    public void CopyTo(Span<byte> destination)
    {
        var compressedContent = AsReadOnlySpan();
        compressedContent.CopyTo(destination);
    }

    public ReadOnlySpan<byte> AsReadOnlySpan()
    {
        using var innerStream = new MemoryStream();
        using var compressionStream = CompressionProvider.GetCompressionStream(innerStream);
        InnerContent.CopyTo(compressionStream);
        compressionStream.Flush();
        innerStream.Seek(0, SeekOrigin.Begin);
        return innerStream.ToArray();
    }
}