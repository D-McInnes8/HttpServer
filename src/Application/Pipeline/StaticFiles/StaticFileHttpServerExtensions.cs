namespace Application.Pipeline.StaticFiles;

/// <summary>
/// Extensions for adding a static file pipeline to the HTTP server
/// </summary>
public static class StaticFileHttpServerExtensions
{
    /// <summary>
    /// Adds a static file pipeline to the HTTP server
    /// </summary>
    /// <param name="httpWebServer">The <see cref="IHttpWebServer"/> to add the request pipeline to.</param>
    /// <param name="configure">Used to configure the static file pipeline.</param>
    /// <returns>The <see cref="IHttpWebServer"/> which can be used to chain methods.</returns>
    public static IHttpWebServer AddStaticFilePipeline(this IHttpWebServer httpWebServer, Action<StaticFilePipelineOptions> configure)
    {
        return httpWebServer.AddPipeline<StaticFilePipelineOptions>(options =>
        {
            options.UseRouter<StaticFileRouter>();
            options.UseRequestHandler<StaticFileRequestHandler>();
            configure(options);
        });
    }
}