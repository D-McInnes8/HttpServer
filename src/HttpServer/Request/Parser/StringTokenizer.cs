using System.Collections;
using System.Diagnostics;

namespace HttpServer.Request.Parser;

public ref struct StringTokenizer
{
    private readonly ReadOnlySpan<char> _s;
    private readonly char[] _delimiters;
    private readonly List<Range> _tokens;
    private int _currentIndex;

    public IReadOnlyCollection<Range> Tokens => _tokens;
    
    public StringTokenizer(ReadOnlySpan<char> s, char[] delimiters)
    {
        Debug.Assert(s.IsEmpty is false);
        Debug.Assert(delimiters.Length > 0);
        
        _s = s;
        _delimiters = delimiters;
        _tokens = CalculateTokens();
        _currentIndex = 0;
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
            if (index < 0 || index >= _tokens.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            var token = _tokens[index];
            return _s[token.Start..token.End];
        }
    }
    
    public string GetString(int index)
    {
        return this[index].ToString();
    }
    
    /// <summary>
    /// Returns the next token in the string. If there are no more tokens, returns null.
    /// </summary>
    /// <returns>The next token in the string, or null if there are no more tokens.</returns>
    public string? GetNextToken()
    {
        return _currentIndex >= _tokens.Count ? null : GetString(_currentIndex++);
    }
    
    /// <summary>
    /// Resets the tokenizer to the beginning of the string.
    /// </summary>
    public void Reset()
    {
        _currentIndex = 0;
    }
}