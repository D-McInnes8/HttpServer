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
        //var max = Math.Min(a.Length, b.Length);
        
        while (i < a.Length && i < b.Length && a[i] == b[i])
        {
            i++;
        }

        return i;
    }
    
    public void AddRoute(Route route, T value)
    {
        if (_rootNode.Children.Length == 0 && route.Path == string.Empty && _rootNode.Value is null)
        {
            _rootNode = new RadixTreeNode<T>
            {
                Prefix = route.Path,
                Children = [],
                Value = value
            };
            return;
        }
        
        if (_rootNode.Children.Length == 0 && route.Path != string.Empty)
        {
            var child = new RadixTreeNode<T>
            {
                Prefix = route.Path,
                Children = [],
                Value = value
            };
            _rootNode.Children = [child];
            return;
        }

        var currentNode = _rootNode;
        var path = route.Path.AsSpan();
        
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
                currentNode = child;
                i = 0;
            }
            
            // Case 3: The node prefix is a partial prefix of the path.
            // Split the node into two nodes.
            if (commonPrefix < child.Prefix.Length
                && commonPrefix < path.Length)
            {
                var parent = new RadixTreeNode<T>
                {
                    Prefix = child.Prefix[..commonPrefix].ToString(),
                    Children = [],
                    Value = default
                };
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
        if (_rootNode.Children.Length == 0)
        {
            return default;
        }
        
        var currentNode = _rootNode;
        var path = route.Path.AsSpan();
        
        for (int i = 0; i < currentNode.Children.Length; i++)
        {
            var child = currentNode.Children[i];
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
                currentNode = child;
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
        var currentNode = _rootNode;
        var path = route.Path.AsSpan();
        
        for (int i = 0; i < currentNode.Children.Length; i++)
        {
            var child = currentNode.Children[i];
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
                currentNode = child;
                i = 0;
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