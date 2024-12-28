namespace Application.Pipeline;

public interface IRequestPipelineBuilder<T> where T : IRequestPipeline
{
    public IServiceProvider Services { get; }
    public IRequestPipelineBuilder<T> ConfigurePipeline(Action<RequestPipelineOptions> configure);
}

public class RequestPipelineBuilder<T> : IRequestPipelineBuilder<T> where T : IRequestPipeline
{
    public IServiceProvider Services { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public RequestPipelineBuilder(IServiceProvider services)
    {
        Services = services;
    }
    
    public IRequestPipelineBuilder<T> ConfigurePipeline(Action<RequestPipelineOptions> configure)
    {
        var pipelineOptions = new RequestPipelineOptions();
        configure(pipelineOptions);
        return this;
    }
}

public class RequestPipelineOptions
{
    public IReadOnlyCollection<Type> Plugins => _plugins;
    
    private readonly List<Type> _plugins = new();
    
    public void AddPlugin<T>() where T : IRequestPipelinePlugin
    {
        _plugins.Add(typeof(T));
    }
    
    public void RemovePlugin<T>() where T : IRequestPipelinePlugin
    {
        _plugins.Remove(typeof(T));
    }
}