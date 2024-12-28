using Application.Request;
using Application.Response;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

public interface IRequestPipelineBuilder<T> where T : IRequestPipeline
{
    public IServiceProvider Services { get; }
    public PipelineRegistry PipelineRegistry { get; }
    public IRequestPipelineBuilder<T> ConfigurePipeline(Action<RequestPipelineOptions> configure);
    public IRequestPipelineBuilder<T> UseDefaultHandler(Func<HttpRequest, Task<HttpResponse>> handler);
}

public class RequestPipelineBuilder<T> : IRequestPipelineBuilder<T> where T : IRequestPipeline
{
    public IServiceProvider Services { get; }
    public PipelineRegistry PipelineRegistry { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public RequestPipelineBuilder(IServiceProvider services)
    {
        Services = services;
        PipelineRegistry = services.GetRequiredService<PipelineRegistry>();
    }
    
    public IRequestPipelineBuilder<T> ConfigurePipeline(Action<RequestPipelineOptions> configure)
    {
        var pipelineOptions = new RequestPipelineOptions(T.DefaultPlugins);
        configure(pipelineOptions);
        
        PipelineRegistry.AddPipeline<T>(plugins: pipelineOptions.Plugins);
        return this;
    }

    public IRequestPipelineBuilder<T> UseDefaultHandler(Func<HttpRequest, Task<HttpResponse>> handler)
    {
        throw new NotImplementedException();
    }
}

public class RequestPipelineOptions
{
    public IReadOnlyCollection<Type> Plugins => _plugins;

    private readonly List<Type> _plugins;

    public RequestPipelineOptions(IReadOnlyCollection<Type> defaultPlugins)
    {
        _plugins = new List<Type>(defaultPlugins);
    }
    
    public void AddPlugin<T>() where T : IRequestPipelinePlugin
    {
        _plugins.Add(typeof(T));
    }
    
    public void RemovePlugin<T>() where T : IRequestPipelinePlugin
    {
        _plugins.Remove(typeof(T));
    }
    
    public void ClearPlugins()
    {
        _plugins.Clear();
    }
}