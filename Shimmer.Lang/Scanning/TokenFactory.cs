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
    
    public Token Create(string lexeme, TokenType type) => new()
        { Lexeme = lexeme, Type = type, Line = _line, Column = _column };

    public Token Plus() => new() { Lexeme = "+", Type = TokenType.Plus, Line = _line, Column = _column };
    public Token Minus() => new() { Lexeme = "-", Type = TokenType.Minus, Line = _line, Column = _column };
    
    public Token Number(string lexeme) => new()
        { Lexeme = lexeme, Type = TokenType.Number, Line = _line, Column = _column };

    public Token Error(string message) => new()
        { Lexeme = message, Type = TokenType.Error, Line = _line, Column = _column };

    public Token Eof() => new() { Lexeme = string.Empty, Type = TokenType.Eof, Line = _line, Column = _column };
}