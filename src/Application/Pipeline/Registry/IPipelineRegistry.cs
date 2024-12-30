namespace Application.Pipeline.Registry;

/// <summary>
/// Represents a collection of request pipeline definitions.
/// </summary>
public interface IPipelineRegistry : IReadOnlyPipelineRegistry
{
    /// <summary>
    /// Adds a new pipeline to the registry.
    /// </summary>
    /// <param name="pipelineName"></param>
    /// <param name="options"></param>
    void AddPipeline(string pipelineName, RequestPipelineBuilderOptions options);
    
    /// <summary>
    /// Removes a pipeline from the registry.
    /// </summary>
    /// <param name="pipelineName"></param>
    void RemovePipeline(string pipelineName);
    
    /// <summary>
    /// Removes all pipelines from the registry.
    /// </summary>
    void Clear();
}