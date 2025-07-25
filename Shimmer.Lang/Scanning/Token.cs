﻿namespace Shimmer.Scanning;

public record Token
{
    public required TokenType Type { get; init; }
    public required string Lexeme { get; init; }
    
    public required int Line { get; init; }
    public required int Column { get; init; }
}