using System.Text;
using HttpServer.Pipeline;
using HttpServer.Response;
using HttpServer.Response.Body;
using Microsoft.Extensions.DependencyInjection;

namespace HttpServer.Routing.StaticFiles;

public static class StaticFileRequestHandler
{
    public static HttpResponse HandleIndividualFile(RequestPipelineContext ctx)
    {
        
        var filePath = ctx.Route?.Metadata["PhysicalPath"];
        if (!File.Exists(filePath))
        {
            return HttpResponse.InternalServerError();
        }
        
        var bytes = File.ReadAllBytes(filePath);
        var contentTypeProvider = ctx.Services.GetRequiredService<IFileContentTypeProvider>();
        var contentType = contentTypeProvider.GetContentType(Path.GetExtension(filePath));
        return HttpResponse.Ok(new ByteArrayBodyContent(bytes, contentType, Encoding.Default));
    }
    
    public static HttpResponse HandleDirectory(RequestPipelineContext ctx)
    {
        var filePath = ctx.Route?.Metadata["PhysicalPath"] + ctx.Request.Route;
        if (!File.Exists(filePath))
        {
            return HttpResponse.NotFound();
        }
        
        var bytes = File.ReadAllBytes(filePath);
        var contentTypeProvider = ctx.Services.GetRequiredService<IFileContentTypeProvider>();
        var contentType = contentTypeProvider.GetContentType(Path.GetExtension(filePath));
        return HttpResponse.Ok(new ByteArrayBodyContent(bytes, contentType, Encoding.Default));
    }
}