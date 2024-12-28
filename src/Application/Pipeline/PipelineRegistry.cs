using System.Collections;

namespace Application.Pipeline;

public class PipelineDefinition
{
    public required string Name { get; init; }
    public required Type PipelineType { get; init; }
    public required IReadOnlyCollection<Type> Plugins { get; init; }
}

public class PipelineRegistry
{
    //private readonly List<HttpRequestPipeline> _pipelines;
    private readonly List<PipelineDefinition> _pipelines;

    internal PipelineRegistry()
    {
        _pipelines = new List<PipelineDefinition>();
    }
    
    public void AddPipeline<T>(string? name = null, IReadOnlyCollection<Type>? plugins = null) where T : IRequestPipeline
    {
        _pipelines.Add(new PipelineDefinition
        {
            Name = name ?? typeof(T).Name,
            PipelineType = typeof(T),
            Plugins = plugins ?? T.DefaultPlugins
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
}