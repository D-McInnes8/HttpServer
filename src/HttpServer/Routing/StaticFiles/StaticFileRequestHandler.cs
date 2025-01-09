using System.Text;
using HttpServer.Pipeline;
using HttpServer.Response;

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
        return HttpResponse.Ok(Encoding.UTF8.GetString(bytes));
    }
    
    public static HttpResponse HandleDirectory(RequestPipelineContext ctx)
    {
        var filePath = ctx.Route?.Metadata["PhysicalPath"] + ctx.Request.Route;
        if (!File.Exists(filePath))
        {
            return HttpResponse.NotFound();
        }
        
        var bytes = File.ReadAllBytes(filePath);
        return HttpResponse.Ok(Encoding.UTF8.GetString(bytes));
    }
}