using System.Text;

namespace HttpServer.Routing;

public interface IRoutingTree<T>
{
    public RadixTreeNode<T> RootNode { get; }
    public void AddRoute(Route path, T value);
    public RouteMatch<T> Match(Route path);
    public string Print();
}

public class RoutingRadixTree<T> : IRoutingTree<T>
{
    private RadixTreeNode<T> _rootNode = new()
    {
        Type = NodeType.Root
    };

    public RadixTreeNode<T> RootNode => _rootNode;

    private int FindCommonPrefix(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        var i = 0;
        while (i < a.Length && i < b.Length && a[i] == b[i])
        {
            i++;
        }
        return i;
    }
    
    public void AddRoute(Route route, T value)
    {
        var path = route.Path.AsSpan();
        
        // If the route contains a wildcard, validate that it is the last segment.
        var wildcardIndex = path.IndexOf("{*}");
        if (wildcardIndex != -1 && wildcardIndex != path.Length - 3)
        {
            throw new ArgumentException("Wildcard segments must be the last segment in the route.");
        }
        
        // If the root node has no children, add the route as a child.
        /*if (_rootNode.Children.Length == 0)
        {
            _rootNode.Children = 
            [
                new RadixTreeNode<T>
                {
                    Prefix = route.Path,
                    Children = [],
                    Value = value,
                    Type = NodeType.Path
                }
            ];
            return;
        }*/

        // Otherwise, iterate through the children of the root node.
        ref var currentNode = ref _rootNode;
        for (int i = 0; i < currentNode.Children.Length; i++)
        {
            ref var child = ref currentNode.Children[i];
            var prefix = child.Type == NodeType.Parameter || child.Type == NodeType.Wildcard
                ? $"{{{child.Prefix}}}".AsSpan()
                : child.Prefix.AsSpan();
            
            // Find the common prefix between the child node and the path.
            var commonPrefix = FindCommonPrefix(prefix, path);
            if (commonPrefix == 0)
            {
                continue;
            }

            // Case 1: The node prefix and the path are equal, so the routes match.
            // Overwrite the value of the node with the new value.
            if (commonPrefix == prefix.Length
                && commonPrefix == path.Length)
            {
                child.Value = value;
                return;
            }
            
            // Case 2: The node prefix is a prefix of the path.
            // Recurse into the child node.
            if (commonPrefix == prefix.Length
                && commonPrefix < path.Length)
            {
                path = path[commonPrefix..];
                currentNode = ref child;
                i = -1;
                continue;
            }
            
            // Case 3: The node prefix is a partial prefix of the path.
            // Split the node into two nodes.
            if (commonPrefix < prefix.Length
                && commonPrefix < path.Length)
            {
                var existingChild = new RadixTreeNode<T>
                {
                    Prefix = child.Prefix[commonPrefix..],
                    Children = child.Children,
                    Value = child.Value,
                    Type = NodeType.Path
                };
                child.Prefix = child.Prefix[..commonPrefix];
                child.Children = [existingChild];
                child.Value = default;
                InsertChild(ref child, path[commonPrefix..], value);
                return;
            }
        }
        
        // If we reach this point, then the current node has no children that match the path.
        // Add a new child node with the path and value.
        InsertChild(ref currentNode, path, value);
    }

    private void InsertChild(ref RadixTreeNode<T> parent, ReadOnlySpan<char> path, T value)
    {
        var nodes = SplitPathIntoNodes(path, value);
        parent.Children = [..parent.Children, nodes];
    }

    private RadixTreeNode<T> SplitPathIntoNodes(ReadOnlySpan<char> path, T value)
    {
        var startIndex = path.IndexOf('{');
        
        // If the path does not contain a parameter, return a new node with the path and value.
        if (startIndex == -1)
        {
            return new RadixTreeNode<T>()
            {
                Prefix = path.ToString(),
                Children = [],
                Value = value,
                Type = NodeType.Path
            };
        }

        // If the path contains a parameter, split the path into two nodes.
        var endIndex = path.IndexOf('}');
        if (endIndex == -1)
        {
            throw new InvalidOperationException("");
        }
        
        var parameter = path[(startIndex + 1)..endIndex];
        var prefix = path[..startIndex];
        var suffix = path[(endIndex + 1)..];
        
        var parameterNode = new RadixTreeNode<T>()
        {
            Prefix = parameter.ToString(),
            Children = [],
            Value = value,
            Type = parameter is "*" ? NodeType.Wildcard : NodeType.Parameter
        };
        if (suffix.Length != 0)
        {
            parameterNode.Value = default;
            parameterNode.Children = [SplitPathIntoNodes(suffix, value)];
        }
        var pathNode = new RadixTreeNode<T>()
        {
            Prefix = prefix.ToString(),
            Children = [parameterNode],
            Value = default,
            Type = NodeType.Path
        };
        return pathNode;
    }

    public RouteMatch<T> Match(Route route)
    {
        ref var currentNode = ref _rootNode;
        var path = route.Path.AsSpan();
        var parameters = new Dictionary<string, string>();
        
        for (int i = 0; i < currentNode.Children.Length; i++)
        {
            ref var child = ref currentNode.Children[i];

            if (child.Type == NodeType.Wildcard)
            {
                parameters.Add(child.Prefix, path.ToString());
                 return RouteMatch<T>.Match(child.Value, parameters);
            }
            if (child.Type == NodeType.Parameter)
            {
                var nextSegmentIndex = path.IndexOf('/');
                if (nextSegmentIndex == -1)
                {
                    nextSegmentIndex = path.Length;
                }
                var segment = path[..nextSegmentIndex];
                parameters.Add(child.Prefix, segment.ToString());
                if (child.Children.Length == 0
                    && nextSegmentIndex == path.Length)
                {
                    return RouteMatch<T>.Match(child.Value, parameters);
                }
                
                path = path[nextSegmentIndex..];
                currentNode = ref child;
                i = -1;
            }
            
            var commonPrefix = FindCommonPrefix(child.Prefix.AsSpan(), path);
            if (commonPrefix == 0)
            {
                continue;
            }

            if (commonPrefix == child.Prefix.Length
                && commonPrefix == path.Length)
            {
                return RouteMatch<T>.Match(child.Value, parameters);
            }
            
            if (commonPrefix == child.Prefix.Length
                && commonPrefix < path.Length)
            {
                path = path[commonPrefix..];
                currentNode = ref child;
                i = -1;
            }
            
            if (commonPrefix < child.Prefix.Length
                && commonPrefix < path.Length)
            {
                
                return RouteMatch<T>.NoMatch;
            }
        }

        return RouteMatch<T>.NoMatch;
    }
    
    public string Print()
    {
        var builder = new StringBuilder();
        
        void PrintNode(RadixTreeNode<T> node, int depth)
        {
            if (node.Type == NodeType.Parameter)
            {
                builder.Append($"{new string(' ', depth * 2)}{{{node.Prefix}}} - {node.Value}\n");
            }
            else
            {
                builder.Append($"{new string(' ', depth * 2)}{node.Prefix} - {node.Value}\n");
            }
            foreach (var child in node.Children)
            {
                PrintNode(child, depth + 1);
            }
        }
        
        PrintNode(_rootNode, 0);
        return builder.ToString();
    }
}

/// <summary>
/// 
/// </summary>
public enum NodeType
{
    /// <summary>
    /// 
    /// </summary>
    Path,
    
    /// <summary>
    /// 
    /// </summary>
    Parameter,
    
    /// <summary>
    /// 
    /// </summary>
    Wildcard,
    
    /// <summary>
    /// 
    /// </summary>
    Root
}

public struct RadixTreeNode<T>()
{
    public string Prefix { get; set; } = string.Empty;
    public T? Value { get; set; } = default;
    public RadixTreeNode<T>[] Children { get; set; } = [];
    public NodeType Type { get; set; } = NodeType.Path;
}

public class RouteMatch<T>
{
    public bool IsMatch { get; set; }
    public T? Value { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();

    private RouteMatch(bool isMatch, T? value)
    {
        IsMatch = isMatch;
        Value = value;
    }

    private RouteMatch(bool isMatch, T? value, Dictionary<string, string> parameters)
    {
        IsMatch = isMatch;
        Value = value;
        Parameters = parameters;
    }
    
    public static RouteMatch<T> NoMatch => new RouteMatch<T>(false, default);
    public static RouteMatch<T> Match(T? value) => new RouteMatch<T>(true, value);
    public static RouteMatch<T> Match(T? value, Dictionary<string, string> parameters) => new RouteMatch<T>(true, value, parameters);
}