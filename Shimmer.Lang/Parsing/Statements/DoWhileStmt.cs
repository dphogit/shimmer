using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class DoWhileStmt(Stmt body, Expr condition) : Stmt
{
    public override string ToString() => $"(do {Body.ToString()} while ({Condition.ToString()}) )";

    public Stmt Body { get; } = body;
    public Expr Condition { get; } = condition;
}