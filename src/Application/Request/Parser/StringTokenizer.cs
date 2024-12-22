using System.Collections;
using System.Diagnostics;

namespace Application.Request.Parser;

public readonly ref struct StringTokenizer
{
    private readonly ReadOnlySpan<char> _s;
    private readonly char[] _delimiters;
    private readonly List<Range> _tokens;

    public IReadOnlyCollection<Range> Tokens => _tokens;
    
    public StringTokenizer(ReadOnlySpan<char> s, char[] delimiters)
    {
        Debug.Assert(s.IsEmpty is false);
        Debug.Assert(delimiters.Length > 0);
        
        _s = s;
        _delimiters = delimiters;
        _tokens = CalculateTokens();
    }

    private List<Range> CalculateTokens()
    {
        List<Range> tokens = [];
        ReadOnlySpan<char> remaining = _s;
        int startPos = 0;

        while (remaining.Length > 0)
        {
            var next = remaining.IndexOfAny(_delimiters);
            if (next == -1)
            {
                tokens.Add(new Range(startPos, startPos + remaining.Length));
                break;
            }

            tokens.Add(new Range(startPos, startPos + next));
            startPos += next + 1;
            remaining = remaining[(next + 1)..];
        }

        return tokens;
    }
    
    public ReadOnlySpan<char> this[int index]
    {
        get
        {
            var token = _tokens[index];
            return _s[token.Start..token.End];
        }
    }
}