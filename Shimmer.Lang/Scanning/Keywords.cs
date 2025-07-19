using System.Collections.ObjectModel;

namespace Shimmer.Scanning;

public static class Keywords
{
    private static readonly ReadOnlyDictionary<string, TokenType> KeywordToType = new(
        new Dictionary<string, TokenType>()
        {
            ["false"] = TokenType.False,
            ["nil"] = TokenType.Nil,
            ["print"] = TokenType.Print,
            ["true"] = TokenType.True,
            ["var"] = TokenType.Var,
        });

    public static TokenType? GetTokenType(string keyword) =>
        KeywordToType.TryGetValue(keyword, out var type) ? type : null;

    public static TokenType[] StatementStarters => [TokenType.Print];
}