using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class WhileStmt(Expr condition, Stmt body, ExprStmt? increment = null) : Stmt
{
    public override string ToString() => $"(while ({Condition.ToString()}) {Body.ToString()} )";
    
    public Expr Condition { get; } = condition;
    public Stmt Body { get; } = body;
    
    /// <summary>
    /// A reference to the increment, which may or may not be in the body already.
    /// This reference needs to be kept tracked of when desugaring a `for` loop for `continue` statements, which
    /// execute the increment after skipping the rest of the body, before moving to the next iteration.
    /// </summary>
    public ExprStmt? Increment { get; } = increment;
}