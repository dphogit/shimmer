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
    And,
    Or,
    
    Number,
    False,
    True,
    Nil,
    Identifier,
    
    Error,
    Eof
}