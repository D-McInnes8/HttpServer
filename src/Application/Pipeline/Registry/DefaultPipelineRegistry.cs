using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Application.Routing;

namespace Application.Pipeline.Registry;

/// <summary>
/// A default implementation of <see cref="IPipelineRegistry"/>, using a simple list as the data store.
/// </summary>
internal class DefaultPipelineRegistry : IPipelineRegistry
{
    private readonly List<IRequestPipeline> _pipelines = new List<IRequestPipeline>();

    /// <summary>
    /// Constructs a new <see cref="DefaultPipelineRegistry"/>.
    /// </summary>
    public DefaultPipelineRegistry()
    {
        var globalPipelineOptions = new RequestPipelineBuilderOptions(null!)
        {
            Name = "Global",
        };
        globalPipelineOptions.UseRouter<DefaultRouter>();
        globalPipelineOptions.UseRequestHandler<GlobalPipelineRequestHandler>();
        GlobalPipeline = new RequestPipeline(globalPipelineOptions);
    }

    /// <inheritdoc />
    public int Count => _pipelines.Count;

    /// <inheritdoc />
    public IRequestPipeline GlobalPipeline { get; }
    
    /// <inheritdoc />
    public bool ContainsPipeline(string pipelineName)
    {
        return _pipelines.Any(p => p.Name == pipelineName);
    }

    /// <inheritdoc />
    public IRequestPipeline? GetPipeline(string pipelineName)
    {
        return _pipelines.FirstOrDefault(pipeline => pipeline.Name == pipelineName);
    }

    /// <inheritdoc />
    public bool TryGetPipeline(string pipelineName, [NotNullWhen(true)] out IRequestPipeline? pipeline)
    {
        foreach (var registryPipeline in _pipelines)
        {
            if (registryPipeline.Name == pipelineName)
            {
                pipeline = registryPipeline;
                return true;
            }
        }

        pipeline = null;
        return false;
    }

    /// <inheritdoc />
    public void AddPipeline(string pipelineName, RequestPipelineBuilderOptions options)
    {
        if (_pipelines.Any(p => p.Name == pipelineName))
        {
            throw new InvalidOperationException($"A pipeline with the name '{pipelineName}' already exists.");
        }
        
        var pipeline = new RequestPipeline(options);
        _pipelines.Add(pipeline);
        
        if (options.Priority != int.MaxValue)
        {
            _pipelines.Sort((a, b) => a.Options.Priority.CompareTo(b.Options.Priority));
        }
    }

    /// <inheritdoc />
    public void RemovePipeline(string pipelineName)
    {
        _pipelines.RemoveAll(p => p.Name == pipelineName);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _pipelines.Clear();
    }

    /// <inheritdoc />
    public IEnumerator<IRequestPipeline> GetEnumerator() => _pipelines.GetEnumerator();
    
    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}