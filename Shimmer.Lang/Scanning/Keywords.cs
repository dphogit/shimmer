using System.Collections.ObjectModel;

namespace Shimmer.Scanning;

public static class Keywords
{
    private static readonly ReadOnlyDictionary<string, TokenType> KeywordToType = new(
        new Dictionary<string, TokenType>()
        {
            ["break"] = TokenType.Break,
            ["case"] = TokenType.Case,
            ["continue"] = TokenType.Continue,
            ["default"] = TokenType.Default,
            ["do"] = TokenType.Do,
            ["false"] = TokenType.False,
            ["for"] = TokenType.For,
            ["if"] = TokenType.If,
            ["else"] = TokenType.Else,
            ["nil"] = TokenType.Nil,
            ["print"] = TokenType.Print,
            ["switch"] = TokenType.Switch,
            ["true"] = TokenType.True,
            ["var"] = TokenType.Var,
            ["while"] = TokenType.While
        });

    public static TokenType? GetTokenType(string keyword) =>
        KeywordToType.TryGetValue(keyword, out var type) ? type : null;

    public static TokenType[] StatementStarters =>
    [
        TokenType.Do, TokenType.For, TokenType.If, TokenType.Print, TokenType.Switch, TokenType.Var, TokenType.While
    ];
}