using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class ExprStmt(Expr expr) : Stmt
{
    public override string ToString() => Expr.ToString();

    public Expr Expr { get; } = expr;
}