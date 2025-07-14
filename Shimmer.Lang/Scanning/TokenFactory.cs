namespace Shimmer.Scanning;

public class TokenFactory(int line = 1, int column = 1)
{
    private int _line = line;
    private int _column = column;

    public void SetColumn(int column)
    {
        _column = column;
    }

    public void SetLine(int line)
    {
        _line = line;
    }

    public Token Create(string lexeme) =>
        lexeme switch
        {
            "+" => Plus(),
            "-" => Minus(),
            "*" => Star(),
            "/" => Slash(),
            "=" => Equal(),
            "(" => LeftParen(),
            ")" => RightParen(),
            "<" => Less(),
            "<=" => LessEqual(),
            ">" => Greater(),
            ">=" => GreaterEqual(),
            "==" => EqualEqual(),
            "!=" => BangEqual(),
            _ => throw new ArgumentException($"Unknown lexeme to create token from: '{lexeme}'", nameof(lexeme))
        };

    public Token Create(string lexeme, TokenType type) => new()
        { Lexeme = lexeme, Type = type, Line = _line, Column = _column };

    public Token Plus() => new() { Lexeme = "+", Type = TokenType.Plus, Line = _line, Column = _column };
    public Token Minus() => new() { Lexeme = "-", Type = TokenType.Minus, Line = _line, Column = _column };
    public Token Star() => new() { Lexeme = "*", Type = TokenType.Star, Line = _line, Column = _column };
    public Token Slash() => new() { Lexeme = "/", Type = TokenType.Slash, Line = _line, Column = _column };
    public Token Equal() => new() { Lexeme = "=", Type = TokenType.Equal, Line = _line, Column = _column };
    public Token Bang() => new() { Lexeme = "!", Type = TokenType.Bang, Line = _line, Column = _column };
    public Token Less() => new() { Lexeme = "<", Type = TokenType.Less, Line = _line, Column = _column };
    public Token LessEqual() => new() { Lexeme = "<=", Type = TokenType.LessEqual, Line = _line, Column = _column };
    public Token Greater() => new() { Lexeme = ">", Type = TokenType.Greater, Line = _line, Column = _column };

    public Token GreaterEqual() =>
        new() { Lexeme = ">=", Type = TokenType.GreaterEqual, Line = _line, Column = _column };

    public Token EqualEqual() => new() { Lexeme = "==", Type = TokenType.EqualEqual, Line = _line, Column = _column };
    public Token BangEqual() => new() { Lexeme = "!=", Type = TokenType.BangEqual, Line = _line, Column = _column };
    public Token LeftParen() => new() { Lexeme = "(", Type = TokenType.LeftParen, Line = _line, Column = _column };
    public Token RightParen() => new() { Lexeme = ")", Type = TokenType.RightParen, Line = _line, Column = _column };
    public Token And() => new() { Lexeme = "&&", Type = TokenType.And, Line =  _line, Column = _column };
    public Token Or() => new() { Lexeme = "||", Type = TokenType.Or, Line =  _line, Column = _column };

    public Token Number(string lexeme) => new()
        { Lexeme = lexeme, Type = TokenType.Number, Line = _line, Column = _column };

    public Token Identifier(string lexeme) => new()
        { Lexeme = lexeme, Type = TokenType.Identifier, Line = _line, Column = _column };

    public Token Nil() => new() { Lexeme = "nil", Type = TokenType.Nil, Line = _line, Column = _column };

    public Token Error(string message) => new()
        { Lexeme = message, Type = TokenType.Error, Line = _line, Column = _column };

    public Token Eof() => new() { Lexeme = string.Empty, Type = TokenType.Eof, Line = _line, Column = _column };
}