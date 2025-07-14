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

        if (IsAlpha(c))
            return IdentifierOrKeyword();

        if (char.IsAsciiDigit(c))
            return Number();

        return c switch
        {
            '+' => _tokenFactory.Plus(),
            '-' => _tokenFactory.Minus(),
            '*' => _tokenFactory.Star(),
            '/' => _tokenFactory.Slash(),
            '(' => _tokenFactory.LeftParen(),
            ')' => _tokenFactory.RightParen(),
            '<' => Check('=', _tokenFactory.LessEqual(), _tokenFactory.Less()),
            '>' => Check('=', _tokenFactory.GreaterEqual(), _tokenFactory.Greater()),
            '=' => Check('=', _tokenFactory.EqualEqual(), _tokenFactory.Equal()),
            '!' => Check('=', _tokenFactory.BangEqual(), _tokenFactory.Bang()),
            '|' => Check('|', _tokenFactory.Or(), _tokenFactory.Error("'|' not supported. Did you mean '||'?")),
            '&' => Check('&', _tokenFactory.And(), _tokenFactory.Error("'&' not supported. Did you mean '&&'?")),
            _ => _tokenFactory.Error($"Unexpected character '{c}'.")
        };
    }

    private Token Check(char c, Token with, Token without)
    {
        if (AtEnd() || source[_cur] != c)
            return without;

        Advance();
        return with;
    }

    private Token Number()
    {
        var startCol = _column - 1;
        
        while (char.IsAsciiDigit(Peek()))
            Advance();

        _tokenFactory.SetColumn(startCol);
        return _tokenFactory.Number(GetLexeme());
    }

    private Token IdentifierOrKeyword()
    {
        var startCol = _column - 1;

        while (IsAlphaNumeric(Peek()))
            Advance();

        var lexeme = GetLexeme();

        var type = Keywords.GetTokenType(lexeme) ?? TokenType.Identifier;
        
        _tokenFactory.SetColumn(startCol);
        return _tokenFactory.Create(lexeme, type);
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
    
    private static bool IsAlpha(char c) => char.IsAsciiLetter(c) ||  c == '_';

    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsAsciiDigit(c);
    
    private string GetLexeme() => source.Substring(_start, _cur - _start);
}