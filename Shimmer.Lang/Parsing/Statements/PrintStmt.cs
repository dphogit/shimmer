using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class PrintStmt(Expr expr) : Stmt
{
    public override string ToString() => $"(print {Expr.ToString()})";

    public Expr Expr { get; } = expr;
}