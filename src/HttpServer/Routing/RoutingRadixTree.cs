using System.Text;

namespace HttpServer.Routing;

public interface IRoutingTree<T>
{
    public void AddRoute(Route path, T value);
    public T? Match(Route path);
    public bool Contains(Route path);
    public string Print();
}

public class RoutingRadixTree<T> : IRoutingTree<T>
{
    private RadixTreeNode<T> _rootNode = new RadixTreeNode<T>();
    
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
        if (_rootNode.Children.Length == 0)
        {
            _rootNode.Children = 
            [
                new RadixTreeNode<T>
                {
                    Prefix = route.Path,
                    Children = [],
                    Value = value
                }
            ];
            return;
        }

        // Otherwise, iterate through the children of the root node.
        ref var currentNode = ref _rootNode;
        for (int i = 0; i < currentNode.Children.Length; i++)
        {
            ref var child = ref currentNode.Children[i];
            var commonPrefix = FindCommonPrefix(child.Prefix.AsSpan(), path);
            if (commonPrefix == 0)
            {
                continue;
            }

            // Case 1: The node prefix and the path are equal, so the routes match.
            // Overwrite the value of the node with the new value.
            if (commonPrefix == child.Prefix.Length
                && commonPrefix == path.Length)
            {
                child.Value = value;
                return;
            }
            
            // Case 2: The node prefix is a prefix of the path.
            // Recurse into the child node.
            if (commonPrefix == child.Prefix.Length
                && commonPrefix < path.Length)
            {
                path = path[commonPrefix..];
                currentNode = ref child;
                i = -1;
                continue;
            }
            
            // Case 3: The node prefix is a partial prefix of the path.
            // Split the node into two nodes.
            if (commonPrefix < child.Prefix.Length
                && commonPrefix < path.Length)
            {
                var existingChild = new RadixTreeNode<T>
                {
                    Prefix = child.Prefix[commonPrefix..],
                    Children = child.Children,
                    Value = child.Value
                };
                var newChild = new RadixTreeNode<T>
                {
                    Prefix = path[commonPrefix..].ToString(),
                    Children = [],
                    Value = value
                };
                child.Prefix = child.Prefix[..commonPrefix];
                child.Children = [existingChild, newChild];
                child.Value = default;
                return;
            }
        }
        
        // If we reach this point, then the current node has no children that match the path.
        // Add a new child node with the path and value.
        var newNode = new RadixTreeNode<T>
        {
            Prefix = path.ToString(),
            Children = [],
            Value = value
        };
        currentNode.Children = [..currentNode.Children, newNode];
    }

    public T? Match(Route route)
    {
        ref var currentNode = ref _rootNode;
        var path = route.Path.AsSpan();
        
        for (int i = 0; i < currentNode.Children.Length; i++)
        {
            ref var child = ref currentNode.Children[i];
            var commonPrefix = FindCommonPrefix(child.Prefix.AsSpan(), path);
            if (commonPrefix == 0)
            {
                continue;
            }

            if (commonPrefix == child.Prefix.Length
                && commonPrefix == path.Length)
            {
                return child.Value;
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
                return default;
            }
        }

        return default;
    }

    public bool Contains(Route route)
    {
        ref var currentNode = ref _rootNode;
        var path = route.Path.AsSpan();
        
        for (int i = 0; i < currentNode.Children.Length; i++)
        {
            ref var child = ref currentNode.Children[i];
            var commonPrefix = FindCommonPrefix(child.Prefix.AsSpan(), path);
            if (commonPrefix == 0)
            {
                continue;
            }

            if (commonPrefix == child.Prefix.Length
                && commonPrefix == path.Length)
            {
                return true;
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
                return false;
            }
        }

        return false;
    }

    public string Print()
    {
        var builder = new StringBuilder();
        
        void PrintNode(RadixTreeNode<T> node, int depth)
        {
            builder.Append($"{new string(' ', depth * 2)}{node.Prefix} - {node.Value}\n");
            foreach (var child in node.Children)
            {
                PrintNode(child, depth + 1);
            }
        }
        
        PrintNode(_rootNode, 0);
        return builder.ToString();
    }
}

public struct RadixTreeNode<T>()
{
    public string Prefix { get; set; } = string.Empty;
    public T? Value { get; set; } = default;
    public RadixTreeNode<T>[] Children { get; set; } = [];
}