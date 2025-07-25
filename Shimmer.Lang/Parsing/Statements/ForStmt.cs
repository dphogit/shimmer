using System.Text;
using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class ForStmt(Stmt? initializer, Expr condition, ExprStmt? increment, Stmt body) : Stmt
{
    public override string ToString()
    {
        var sb = new StringBuilder("(for ");

        sb.Append(Initializer is not null ? Initializer.ToString() : "()").Append(' ');
        sb.Append(Condition.ToString()).Append(' ');
        sb.Append(Increment is not null ? Increment.ToString() : "()").Append(' ');
        
        sb.Append(Body.ToString());
        
        return sb.Append(')').ToString();
    }

    public Stmt? Initializer { get; } = initializer;
    public Expr Condition { get; } = condition;
    public ExprStmt? Increment { get; } = increment;
    public Stmt Body { get; } = body;
}