using Shimmer.Scanning;

namespace Shimmer.Parsing.Expressions;

public class AssignExpr(Token name, Expr value) : Expr
{
    public override string ToString() => $"({Name.Lexeme} = {Value.ToString()})";

    public Token Name { get; } = name;
    public Expr Value { get; } = value;
}