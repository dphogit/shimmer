namespace Shimmer.Scanning;

public enum TokenType
{
    Plus,
    Minus,
    Star,
    Slash,
    Equal,
    Bang,
    Less,
    LessEqual,
    EqualEqual,
    BangEqual,
    Greater,
    GreaterEqual,
    LeftParen,
    RightParen,
    
    Number,
    False,
    True,
    Identifier,
    
    Error,
    Eof
}