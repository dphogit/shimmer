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
            "%" => Remainder(),
            "=" => Equal(),
            "(" => LeftParen(),
            ")" => RightParen(),
            "<" => Less(),
            "<=" => LessEqual(),
            ">" => Greater(),
            ">=" => GreaterEqual(),
            "==" => EqualEqual(),
            "!=" => BangEqual(),
            "!" => Bang(),
            "&&" => And(),
            "||" => Or(),
            "," => Comma(),
            ":" => Colon(),
            "?" => Question(),
            "nil" => Nil(),
            _ => throw new ArgumentException($"Unknown lexeme to create token from: '{lexeme}'", nameof(lexeme))
        };

    public Token Create(string lexeme, TokenType type) => new()
        { Lexeme = lexeme, Type = type, Line = _line, Column = _column };

    public Token Plus() => Create("+", TokenType.Plus);
    public Token Minus() => Create("-", TokenType.Minus);
    public Token Star() => Create("*", TokenType.Star);
    public Token Slash() => Create("/", TokenType.Slash);
    public Token Remainder() => Create("%", TokenType.Percent);
    public Token Equal() => Create("=", TokenType.Equal);
    public Token Bang() => Create("!", TokenType.Bang);
    public Token Less() => Create("<", TokenType.Less);
    public Token LessEqual() => Create("<=", TokenType.LessEqual);
    public Token Greater() => Create(">", TokenType.Greater);
    public Token GreaterEqual() => Create(">=", TokenType.GreaterEqual);
    public Token EqualEqual() => Create("==", TokenType.EqualEqual);
    public Token BangEqual() => Create("!=", TokenType.BangEqual);
    public Token LeftParen() => Create("(", TokenType.LeftParen);
    public Token RightParen() => Create(")", TokenType.RightParen);
    public Token And() => Create("&&", TokenType.And);
    public Token Or() => Create("||", TokenType.Or);
    public Token Comma() => Create(",", TokenType.Comma);
    public Token Colon() => Create(":", TokenType.Colon);
    public Token Question() => Create("?", TokenType.Question);
    public Token Number(string lexeme) => Create(lexeme, TokenType.Number);
    public Token Identifier(string lexeme) => Create(lexeme, TokenType.Identifier);
    public Token String(string lexeme) => Create(lexeme, TokenType.String);
    public Token Nil() => Create("nil", TokenType.Nil);
    public Token Error(string message) => Create(message, TokenType.Error);
    public Token Eof() => Create(string.Empty, TokenType.Eof);
}