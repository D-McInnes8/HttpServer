using Application.Routing;

namespace Application.Pipeline;

/// <summary>
/// 
/// </summary>
public class RequestPipelineBuilderOptions
{
    private readonly List<Type> _plugins = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public RequestPipelineBuilderOptions(IServiceProvider services)
    {
        Services = services;
    }

    public IServiceProvider Services { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyCollection<Type> Plugins => _plugins;

    /// <summary>
    /// 
    /// </summary>
    public Type Router { get; private set; } = typeof(DefaultRouter);

    /// <summary>
    /// 
    /// </summary>
    public Type RequestHandler { get; private set; } = typeof(DefaultRequestHandler);

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = $"{Guid.NewGuid():N}";

    /// <summary>
    /// 
    /// </summary>
    public int Priority { get; set; } = int.MaxValue;
    
    /// <summary>
    /// 
    /// </summary>
    public string? PathPrefix { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TRouter"></typeparam>
    public void UseRouter<TRouter>() where TRouter : IRouter
    {
        Router = typeof(TRouter);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="THandler"></typeparam>
    public void UseRequestHandler<THandler>() where THandler : IRequestHandler
    {
        RequestHandler = typeof(THandler);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pipelineName"></param>
    public void WithName(string pipelineName)
    {
        Name = pipelineName;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="priority"></param>
    public void WithPriority(int priority)
    {
        Priority = priority;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pathPrefix"></param>
    public void WithPathPrefix(string pathPrefix)
    {
        PathPrefix = pathPrefix;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    public void AddPlugin<TPlugin>() where TPlugin : IRequestPipelinePlugin
    {
        _plugins.Add(typeof(TPlugin));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pluginType"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddPlugin(Type pluginType)
    {
        if (!typeof(IRequestPipelinePlugin).IsAssignableFrom(pluginType))
        {
            throw new ArgumentException("The plugin type must implement IRequestPipelinePlugin.", nameof(pluginType));
        }
        
        _plugins.Add(pluginType);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    public void RemovePlugin<TPlugin>() where TPlugin : IRequestPipelinePlugin
    {
        _plugins.Remove(typeof(TPlugin));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pluginType"></param>
    public void RemovePlugin(Type pluginType)
    {
        _plugins.Remove(pluginType);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void ClearPlugins()
    {
        _plugins.Clear();
    }
}