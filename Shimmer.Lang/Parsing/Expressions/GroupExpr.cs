namespace Shimmer.Parsing.Expressions;

public class GroupExpr(Expr expr) : Expr
{
    public override string ToString() => Expr.ToString();

    public Expr Expr { get; } = expr;
}