using Shimmer.Scanning;

namespace Shimmer.Parsing.Expressions;

public class VarExpr(Token name) : Expr
{
    public override string ToString() => Name.Lexeme;

    public Token Name { get; } = name;
}