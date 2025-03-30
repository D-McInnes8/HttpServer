using HttpServer.Pipeline;

namespace HttpServer.Plugins.ResponseCompression.DependencyInjection;

/// <summary>
/// Extensions for the <see cref="RequestPipelineBuilderOptions"/> to add the response compression plugin.
/// </summary>
public static class ResponseCompressionPipelineBuilderExtensions
{
    /// <summary>
    /// Adds the response compression plugin to the pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline that the response compression plugin should be added to.</param>
    /// <returns>The pipeline for chaining calls.</returns>
    public static RequestPipelineBuilderOptions UseResponseCompression(this RequestPipelineBuilderOptions pipeline)
    {
        pipeline.AddPlugin<ResponseCompressionPlugin>();
        return pipeline;
    }
}