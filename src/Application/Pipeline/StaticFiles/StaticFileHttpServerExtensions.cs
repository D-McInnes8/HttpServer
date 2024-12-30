namespace Application.Pipeline.StaticFiles;

public static class StaticFileHttpServerExtensions
{
    public static IHttpServer AddStaticFilePipeline(this IHttpServer httpServer, Action<StaticFilePipelineOptions> configure)
    {
        return httpServer.AddPipeline<StaticFilePipelineOptions>(options =>
        {
            options.UseRouter<StaticFileRouter>();
            options.UseRequestHandler<StaticFileRequestHandler>();
            configure(options);
        });
    }
}