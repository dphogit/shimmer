namespace Shimmer.Scanning;

public enum TokenType
{
    // Single Character
    Plus,
    Minus,
    Star,
    Slash,
    Percent,
    Equal,
    Bang,
    Less,
    Greater,
    LeftParen,
    RightParen,
    Comma,
    Colon,
    SemiColon,
    Question,
    
    // Two characters
    LessEqual,
    EqualEqual,
    BangEqual,
    GreaterEqual,
    
    // Literals
    Number,
    Identifier,
    String,
    
    // Keywords
    Print,
    And,
    Or,
    False,
    True,
    Nil,

    // Misc
    Error,
    Eof
}