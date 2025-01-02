namespace HttpServer.Routing;

public interface IRoutingTree<T>
{
    public void AddRoute(Route path, T value);
    public T? Match(Route path);
    public bool Contains(Route path);
}

public class RoutingRadixTree<T> : IRoutingTree<T>
{
    private RadixTreeNode<T>[] _rootNodes = [];
    
    public void AddRoute(Route path, T value)
    {
        throw new NotImplementedException();
    }

    public T? Match(Route path)
    {
        throw new NotImplementedException();
    }

    public bool Contains(Route path)
    {
        throw new NotImplementedException();
    }
}

public struct RadixTreeNode<T>
{
    public string Prefix { get; set; }
    public T? Value { get; set; }
    public RadixTreeNode<T>[] Children { get; set; }
}