using System.Collections;
using Application.Pipeline.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

public class PipelineDefinition
{
    public required string Name { get; init; }
    public required Type PipelineType { get; init; }
    public required IReadOnlyCollection<Type> Plugins { get; set; }
}

public class PipelineRegistry
{
    public IReadOnlyCollection<PipelineDefinition> Pipelines => _pipelines;
    
    //private readonly List<HttpRequestPipeline> _pipelines;
    private readonly List<PipelineDefinition> _pipelines;

    internal PipelineRegistry()
    {
        _pipelines = new List<PipelineDefinition>();
    }
    
    public void AddPipeline<T>(string name) where T : IRequestPipeline
    {
        if (_pipelines.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"A pipeline with the name '{name}' already exists.", nameof(name));
        }
        
        _pipelines.Add(new PipelineDefinition
        {
            Name = name,
            PipelineType = typeof(T),
            Plugins = T.DefaultPlugins
        });
    }

    public void RemovePipeline<T>() where T : IRequestPipeline
    {
        _pipelines.RemoveAll(p => p.PipelineType == typeof(T));
    }
    
    public void RemovePipeline(string name)
    {
        _pipelines.RemoveAll(p => p.Name == name);
    }

    public void Clear()
    {
        _pipelines.Clear();
    }

    public void UpdatePipeline(string name, Action<PipelineDefinition> configure)
    {
        configure(_pipelines.First(p => p.Name == name));
    }
    
    /*internal void AddPipeline(HttpRequestPipeline requestPipeline)
    {
        _pipelines.Add(requestPipeline);
    }
    
    internal void RemovePipeline(HttpRequestPipeline requestPipeline)
    {
        _pipelines.Remove(requestPipeline);
    }

    public IEnumerator<HttpRequestPipeline> GetEnumerator()
    {
        return _pipelines.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }*/

    public IEnumerator<PipelineDefinition> GetEnumerator() => _pipelines.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _pipelines.Count;
    
    public bool ContainsPipeline(string pipelineName)
    {
        return _pipelines.Any(p => string.Equals(pipelineName, p.Name, StringComparison.OrdinalIgnoreCase));
    }
}