namespace Application.Pipeline.StaticFiles;

/// <summary>
/// Extensions for adding a static file pipeline to the HTTP server
/// </summary>
public static class StaticFileHttpServerExtensions
{
    /// <summary>
    /// Adds a static file pipeline to the HTTP server
    /// </summary>
    /// <param name="httpServer">The <see cref="IHttpServer"/> to add the request pipeline to.</param>
    /// <param name="configure">Used to configure the static file pipeline.</param>
    /// <returns>The <see cref="IHttpServer"/> which can be used to chain methods.</returns>
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