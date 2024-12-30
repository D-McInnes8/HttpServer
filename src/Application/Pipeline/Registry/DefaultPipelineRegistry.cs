using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Application.Routing;

namespace Application.Pipeline.Registry;

/// <inheritdoc />
internal class DefaultPipelineRegistry : IPipelineRegistry
{
    private readonly List<IRequestPipeline> _pipelines = new List<IRequestPipeline>();

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

    public int Count => _pipelines.Count;

    public IRequestPipeline GlobalPipeline { get; }
    
    public bool ContainsPipeline(string pipelineName)
    {
        return _pipelines.Any(p => p.Name == pipelineName);
    }

    public IRequestPipeline? GetPipeline(string pipelineName)
    {
        return _pipelines.FirstOrDefault(pipeline => pipeline.Name == pipelineName);
    }

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

    public void AddPipeline(string pipelineName, RequestPipelineBuilderOptions options)
    {
        if (_pipelines.Any(p => p.Name == pipelineName))
        {
            throw new InvalidOperationException($"A pipeline with the name '{pipelineName}' already exists.");
        }
        
        var pipeline = new RequestPipeline(options);
        _pipelines.Add(pipeline);
    }

    public void RemovePipeline(string pipelineName)
    {
        _pipelines.RemoveAll(p => p.Name == pipelineName);
    }

    public void Clear()
    {
        _pipelines.Clear();
    }

    public IEnumerator<IRequestPipeline> GetEnumerator() => _pipelines.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}