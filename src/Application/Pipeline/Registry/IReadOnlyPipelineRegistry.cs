using System.Diagnostics.CodeAnalysis;

namespace Application.Pipeline.Registry;

/// <summary>
/// Represents a read-only collection of request pipeline definitions.
/// </summary>
public interface IReadOnlyPipelineRegistry : IEnumerable<IRequestPipeline>
{
    /// <summary>
    /// Gets the total number of pipelines in the registry.
    /// </summary>
    public int Count { get; }
    
    /// <summary>
    /// Gets the global request pipeline.
    /// </summary>
    public IRequestPipeline GlobalPipeline { get; }
    
    /// <summary>
    /// Determines whether the registry contains a pipeline with the specified name.
    /// </summary>
    /// <param name="pipelineName">The pipeline name to search for in the registry.</param>
    /// <returns>True if <paramref name="pipelineName"/> exists otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipelineName"/> is <see langword="null"/>.</exception>
    bool ContainsPipeline(string pipelineName);

    /// <summary>
    /// Gets the pipeline with the specified name.
    /// </summary>
    /// <param name="pipelineName">The pipeline name to search for in the registry.</param>
    /// <returns>The pipeline if it exists in the registry, otherwise <see langword="null"/>.</returns>
    public IRequestPipeline? GetPipeline(string pipelineName);
    
    /// <summary>
    /// Tries to get the pipeline with the specified name.
    /// </summary>
    /// <param name="pipelineName">The pipeline name to search for in the registry.</param>
    /// <param name="pipeline">The pipeline if it exists in the registry, otherwise <see langword="null"/>.</param>
    /// <returns>True if <paramref name="pipelineName"/> exists otherwise false.</returns>
    public bool TryGetPipeline(string pipelineName, [NotNullWhen(true)] out IRequestPipeline? pipeline);
    
    /// <summary>
    /// Gets the pipeline with the specified name.
    /// </summary>
    /// <param name="pipelineName">The pipeline name to search for in the registry.</param>
    public IRequestPipeline? this[string pipelineName] => GetPipeline(pipelineName);
}