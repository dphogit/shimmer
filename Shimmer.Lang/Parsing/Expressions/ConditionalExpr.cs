namespace Shimmer.Parsing.Expressions;

public class ConditionalExpr(Expr condition, Expr thenExpr, Expr elseExpr) : Expr
{
    public override string ToString() => $"({Condition.ToString()} ? {ThenExpr.ToString()} : {ElseExpr.ToString()})";

    public Expr Condition { get; } = condition;
    public Expr ThenExpr { get; } = thenExpr;
    public Expr ElseExpr { get; } = elseExpr;
}