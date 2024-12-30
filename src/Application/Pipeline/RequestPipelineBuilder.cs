using Application.Request;
using Application.Response;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Pipeline;

public class RequestPipelineBuilderOptions
{
    public IReadOnlyCollection<Type> Plugins { get; set; }
    
    public Type Router { get; protected set; }
    
    public Type RequestHandler { get; protected set; }
    
    public string Name { get; set; }
    
    public int Priority { get; set; }
    
    public string? PathPrefix { get; set; }
    
    public RequestPipelineBuilderOptions()
    {
        Plugins = new List<Type>();
    }
    
    public void UseRouter<TRouter>() where TRouter : IRequestPipelinePlugin
    {
        Router = typeof(TRouter);
    }
    
    public void UseRequestHandler<THandler>() where THandler : IRequestPipelinePlugin
    {
        RequestHandler = typeof(THandler);
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRequestPipelineBuilder<T> where T : RequestPipelineBuilderOptions
{
    /// <summary>
    /// 
    /// </summary>
    public IServiceProvider Services { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public T Options { get; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TRouter"></typeparam>
    /// <returns></returns>
    public IRequestPipelineBuilder<T> UseRouter<TRouter>() where TRouter : IRequestPipelinePlugin;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    public IRequestPipelineBuilder<T> ConfigurePlugins(Action<RequestPipelineOptions> configure);
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="THandler"></typeparam>
    /// <returns></returns>
    public IRequestPipelineBuilder<T> UseRequestHandler<THandler>() where THandler : IRequestPipelinePlugin;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IRequestPipelineBuilder<T> WithName(string name);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="priority"></param>
    /// <returns></returns>
    public IRequestPipelineBuilder<T> WithPriority(int priority);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pathPrefix"></param>
    /// <returns></returns>
    public IRequestPipelineBuilder<T> WithPathPrefix(string pathPrefix);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    internal PipelineDefinition Build();
}

public class RequestPipelineBuilder<T> : IRequestPipelineBuilder<T> where T : RequestPipelineBuilderOptions
{
    public RequestPipelineBuilder(IServiceProvider services)
    {
        Services = services;
    }

    public IServiceProvider Services { get; }
    
    public IRequestPipelineBuilder<T> UseRouter<TRouter>() where TRouter : IRequestPipelinePlugin
    {
        throw new NotImplementedException();
    }

    public IRequestPipelineBuilder<T> ConfigurePlugins(Action<RequestPipelineOptions> configure)
    {
        throw new NotImplementedException();
    }

    public IRequestPipelineBuilder<T> UseRequestHandler<THandler>() where THandler : IRequestPipelinePlugin
    {
        throw new NotImplementedException();
    }

    public IRequestPipelineBuilder<T> WithName(string name)
    {
        throw new NotImplementedException();
    }

    public IRequestPipelineBuilder<T> WithPriority(int priority)
    {
        throw new NotImplementedException();
    }

    public IRequestPipelineBuilder<T> WithPathPrefix(string pathPrefix)
    {
        throw new NotImplementedException();
    }

    public PipelineDefinition Build()
    {
        throw new NotImplementedException();
    }
}




public interface IRequestPipelineBuilder2<T> where T : IRequestPipeline
{
    public IServiceProvider Services { get; }
    public PipelineRegistry PipelineRegistry { get; }
    //public IRequestPipelineBuilder<T> UseRouter<TRouter>() where TRouter : IRequestPipelinePlugin;
    public IRequestPipelineBuilder2<T> ConfigurePipeline(Action<RequestPipelineOptions> configure);
    //public IRequestPipelineBuilder<T> ConfigurePlugins(Action<RequestPipelineOptions> configure);
    public IRequestPipelineBuilder2<T> UseDefaultHandler(Func<HttpRequest, Task<HttpResponse>> handler);
    /*public IRequestPipelineBuilder<T> UseRequestHandler<THandler>() where THandler : IRequestPipelinePlugin;
    public IRequestPipelineBuilder<T> WithName(string name);
    public IRequestPipelineBuilder<T> WithPriority(int priority);
    public IRequestPipelineBuilder<T> WithPathPrefix(string pathPrefix);*/
}

public class RequestPipelineBuilder2<T> : IRequestPipelineBuilder2<T> where T : IRequestPipeline
{
    public string PipelineName { get; }
    public IServiceProvider Services { get; }
    public PipelineRegistry PipelineRegistry { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pipelineName"></param>
    /// <param name="services"></param>
    public RequestPipelineBuilder2(string pipelineName, IServiceProvider services)
    {
        PipelineName = pipelineName;
        Services = services;
        PipelineRegistry = services.GetRequiredService<PipelineRegistry>();
    }
    
    public IRequestPipelineBuilder2<T> ConfigurePipeline(Action<RequestPipelineOptions> configure)
    {
        var pipelineOptions = new RequestPipelineOptions(T.DefaultPlugins);
        configure(pipelineOptions);

        PipelineRegistry.UpdatePipeline(PipelineName, (p) =>
        {
            p.Plugins = pipelineOptions.Plugins;
        });
        //PipelineRegistry.AddPipeline<T>(plugins: pipelineOptions.Plugins);
        return this;
    }

    public IRequestPipelineBuilder2<T> UseDefaultHandler(Func<HttpRequest, Task<HttpResponse>> handler)
    {
        throw new NotImplementedException();
    }
}

public class RequestPipelineOptions
{
    public IReadOnlyCollection<Type> Plugins => _plugins;

    private readonly List<Type> _plugins;
    private readonly IReadOnlyCollection<Type> _defaultPlugins;

    public RequestPipelineOptions(IReadOnlyCollection<Type> defaultPlugins)
    {
        _defaultPlugins = defaultPlugins;
        _plugins = new List<Type>(_defaultPlugins);
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

    public void ResetPlugins()
    {
        _plugins.Clear();
        _plugins.AddRange(_defaultPlugins);
    }
}