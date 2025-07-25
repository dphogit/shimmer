using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class WhileStmt(Expr condition, Stmt body) : Stmt
{
    public override string ToString() => $"(while ({Condition.ToString()}) {Body.ToString()} )";
    
    public Expr Condition { get; } = condition;
    public Stmt Body { get; } = body;
}