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
        var errorToken = SkipWhitespace();
        if (errorToken is not null)
            return errorToken;

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
            '%' => _tokenFactory.Remainder(),
            '(' => _tokenFactory.LeftParen(),
            ')' => _tokenFactory.RightParen(),
            ',' => _tokenFactory.Comma(),
            ':' => _tokenFactory.Colon(),
            ';' => _tokenFactory.SemiColon(),
            '?' => _tokenFactory.Question(),
            '"' => String(),
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

    private Token String()
    {
        var startCol = _column - 1;

        while (Peek() != '"' && !AtEnd())
        {
            if (Peek() == '\n')
                NextLine();
            else
                Advance();
        }


        if (AtEnd())
        {
            _tokenFactory.SetColumn(startCol);
            return _tokenFactory.Error("Unterminated string.");
        }

        Advance();  // Consume closing "
        
        var lexeme = GetLexeme();
        
        _tokenFactory.SetColumn(startCol);
        return _tokenFactory.String(lexeme);
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

    // Returns an error token if there is an error scanning whitespace, otherwise null (success)
    private Token? SkipWhitespace()
    {
        while (true)
        {
            switch (Peek())
            {
                case '\n':
                {
                    NextLine();
                    continue;
                }
                case '/':
                {
                    var next = PeekNext();

                    if (next != '/' && next != '*')
                        return null;

                    if (next == '/')
                    {
                        InlineComment();
                        continue;
                    }

                    var errorToken = BlockComment();
                    if (errorToken is not null)
                        return errorToken;

                    continue;
                }
                case ' ':
                case '\t':
                case '\r':
                    Advance();
                    break;
                default:
                    return null;
            }
        }
    }

    private void NextLine()
    {
        Advance(); // Consume '\n'
        SetLine(_line + 1);
        SetColumn(1);
    }

    private void InlineComment()
    {
        // Consume '//'
        Advance();
        Advance();

        while (Peek() != '\n' && !AtEnd())
            Advance();
    }

    private Token? BlockComment()
    {
        var commentStartColumn = _column;
        var commentStartLine = _line;

        // Consume the opening '/*'
        Advance();
        Advance();

        while (!AtEnd() && !(Peek() == '*' && PeekNext() == '/'))
        {
            if (Peek() == '\n')
                NextLine();
            else
                Advance();
        }

        if (AtEnd())
        {
            _tokenFactory.SetColumn(commentStartColumn);
            _tokenFactory.SetLine(commentStartLine);
            return _tokenFactory.Error("Unterminated block comment.");
        }

        // Consume the closing '*/'
        Advance();
        Advance();

        return null;
    }

    private char Peek() => AtEnd() ? '\0' : source[_cur];
    private char PeekNext() => _cur + 1 >= source.Length ? '\0' : source[_cur + 1];

    private bool AtEnd() => _cur >= source.Length;

    private static bool IsAlpha(char c) => char.IsAsciiLetter(c) || c == '_';

    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsAsciiDigit(c);

    private string GetLexeme() => source.Substring(_start, _cur - _start);

    private void SetLine(int line)
    {
        _line = line;
        _tokenFactory.SetLine(line);
    }

    private void SetColumn(int column)
    {
        _column = column;
        _tokenFactory.SetColumn(column);
    }
}