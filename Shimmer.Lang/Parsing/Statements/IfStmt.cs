using System.Text;
using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class IfStmt(Expr condition, Stmt thenBranch, Stmt? elseBranch = null) : Stmt
{
    public override string ToString()
    {
        var sb = new StringBuilder($"(if ({Condition.ToString()}) (then {ThenBranch.ToString()})");

        if (ElseBranch is null)
        {
            sb.Append(" )");
            return sb.ToString();
        }

        sb.Append($" (else {ElseBranch.ToString()}) )");
        return sb.ToString();
    }
    
    public Expr Condition { get; } = condition;
    public Stmt ThenBranch { get; } = thenBranch;
    public Stmt? ElseBranch { get; } = elseBranch;
}