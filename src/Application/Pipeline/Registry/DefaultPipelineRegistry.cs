using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Application.Pipeline.Registry;

/// <inheritdoc />
internal class DefaultPipelineRegistry : IPipelineRegistry
{
    private readonly List<IRequestPipeline3> _pipelines = new List<IRequestPipeline3>();

    public DefaultPipelineRegistry()
    {
        var globalPipelineOptions = new RequestPipelineBuilderOptions()
        {
            Name = "Global",
        };
        GlobalPipeline = new RequestPipeline(globalPipelineOptions);
    }

    public int Count => _pipelines.Count;

    public IRequestPipeline3 GlobalPipeline { get; }
    
    public bool ContainsPipeline(string pipelineName)
    {
        return _pipelines.Any(p => p.Name == pipelineName);
    }

    public IRequestPipeline3? GetPipeline(string pipelineName)
    {
        return _pipelines.FirstOrDefault(pipeline => pipeline.Name == pipelineName);
    }

    public bool TryGetPipeline(string pipelineName, [NotNullWhen(true)] out IRequestPipeline3? pipeline)
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

    public IEnumerator<IRequestPipeline3> GetEnumerator() => _pipelines.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}