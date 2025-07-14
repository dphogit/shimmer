using Shimmer.Scanning;

namespace Shimmer.Parsing.Expressions;

public class UnaryExpr(Token op, Expr expr) : Expr
{
    public override string ToString() => $"({Op.Lexeme} {Expr.ToString()})";

    public Token Op { get; } = op;
    public Expr Expr { get; } = expr;
}