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
    LeftBrace,
    RightBrace,
    
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
    If,
    Else,
    True,
    Var,
    While,
    For,
    Break,
    Continue,
    Do,
    Switch,
    Case,
    Default,
    Nil,

    // Misc
    Error,
    Eof
}