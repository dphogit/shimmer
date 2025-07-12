namespace Shimmer.Scanning;

public class Scanner(string source) : IScanner
{
    private int _start = 0;
    private int _cur = 0;
    private int _line = 1;
    private int _column = 1;

    private readonly TokenFactory _tokenFactory = new();

    public Token NextToken()
    {
        SkipWhitespace();
        _start = _cur;
        
        if (AtEnd())
            return _tokenFactory.Eof();
        
        var c = Advance();

        if (char.IsAsciiDigit(c))
            return Number();

        return c switch
        {
            '+' => _tokenFactory.Plus(),
            '-' => _tokenFactory.Minus(),
            _ => _tokenFactory.Error($"Unexpected character '{c}'.")
        };
    }

    private Token Number()
    {
        var startCol = _column - 1;
        
        while (char.IsAsciiDigit(Peek()))
            Advance();

        _tokenFactory.SetColumn(startCol);
        return _tokenFactory.Number(source.Substring(_start, _cur - _start));
    }

    private char Advance()
    {
        _tokenFactory.SetColumn(_column++);
        return source[_cur++];
    }

    private void SkipWhitespace()
    {
        while (char.IsWhiteSpace(Peek()))
        {
            if (Peek() == '\n')
            {
                Advance();
                
                _tokenFactory.SetLine(++_line);
                
                _column = 1;
                _tokenFactory.SetColumn(1);
                
                continue;
            }

            Advance();
        }
    }

    private char Peek() => AtEnd() ? '\0' : source[_cur];

    private bool AtEnd() => _cur >= source.Length;
}