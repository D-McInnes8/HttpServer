using Application.Routing;

namespace Application.Pipeline;

/// <summary>
/// Options for configuring a request pipeline. This class can be derived from to create custom request pipeline options.
/// </summary>
public class RequestPipelineBuilderOptions
{
    private readonly List<Type> _plugins = new();

    /// <summary>
    /// Creates a new instance of <see cref="RequestPipelineBuilderOptions"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> to be used by the options.</param>
    public RequestPipelineBuilderOptions(IServiceProvider services)
    {
        Services = services;
    }

    /// <summary>
    /// The <see cref="IServiceProvider"/> associated with this request pipeline.
    /// </summary>
    public IServiceProvider Services { get; }
    
    /// <summary>
    /// The plugins that are associated with this request pipeline.
    /// </summary>
    public IReadOnlyCollection<Type> Plugins => _plugins;

    /// <summary>
    /// The router that will be used to route requests to the appropriate pipeline.
    /// </summary>
    public Type Router { get; private set; } = typeof(DefaultRouter);

    /// <summary>
    /// The request handler that will be used to handle requests that are routed to this pipeline.
    /// </summary>
    public Type RequestHandler { get; private set; } = typeof(DefaultRequestHandler);

    /// <summary>
    /// The name of the pipeline. Each pipeline must have a unique name.
    /// </summary>
    public string Name { get; set; } = $"{Guid.NewGuid():N}";

    /// <summary>
    /// The priority of the pipeline. Pipelines with lower priority values will be executed first. The default value is <see cref="int.MaxValue"/>.
    /// </summary>
    public int Priority { get; set; } = int.MaxValue;
    
    /// <summary>
    /// The path prefix that will be used to route requests to this pipeline.
    /// </summary>
    public string? PathPrefix { get; set; }
    
    /// <summary>
    /// Sets the router that will be used to route requests to the appropriate pipeline.
    /// </summary>
    /// <typeparam name="TRouter"></typeparam>
    public void UseRouter<TRouter>() where TRouter : IRouter
    {
        Router = typeof(TRouter);
    }
    
    /// <summary>
    /// Sets the request handler that will be used to handle requests that are routed to this pipeline.
    /// </summary>
    /// <typeparam name="THandler"></typeparam>
    public void UseRequestHandler<THandler>() where THandler : IRequestHandler
    {
        RequestHandler = typeof(THandler);
    }

    /// <summary>
    /// Sets the name of the pipeline.
    /// </summary>
    /// <param name="pipelineName"></param>
    public void WithName(string pipelineName)
    {
        Name = pipelineName;
    }
    
    /// <summary>
    /// Sets the priority of the pipeline.
    /// </summary>
    /// <param name="priority"></param>
    public void WithPriority(int priority)
    {
        Priority = priority;
    }
    
    /// <summary>
    /// Sets the path prefix that will be used to route requests to this pipeline.
    /// </summary>
    /// <param name="pathPrefix"></param>
    public void WithPathPrefix(string pathPrefix)
    {
        PathPrefix = pathPrefix;
    }
    
    /// <summary>
    /// Adds a plugin to the pipeline.
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    public void AddPlugin<TPlugin>() where TPlugin : IRequestPipelinePlugin
    {
        _plugins.Add(typeof(TPlugin));
    }
    
    /// <summary>
    /// Adds a plugin to the pipeline.
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
    /// Removes a plugin from the pipeline.
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    public void RemovePlugin<TPlugin>() where TPlugin : IRequestPipelinePlugin
    {
        _plugins.Remove(typeof(TPlugin));
    }
    
    /// <summary>
    /// Removes a plugin from the pipeline.
    /// </summary>
    /// <param name="pluginType"></param>
    public void RemovePlugin(Type pluginType)
    {
        _plugins.Remove(pluginType);
    }
    
    /// <summary>
    /// Clears all plugins from the pipeline.
    /// </summary>
    public void ClearPlugins()
    {
        _plugins.Clear();
    }
}