using System.Text;
using Shimmer.Parsing.Expressions;

namespace Shimmer.Parsing.Statements;

public class SwitchStmt(Expr expr, IList<SwitchStmt.CaseClause> cases, Stmt? defaultClause) : Stmt
{
    public class CaseClause(Expr condition, Stmt stmt)
    {
        public Expr Condition { get;  } = condition;
        public Stmt Stmt { get; } = stmt;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder($"(switch ({Expr.ToString()})");

        foreach (var c in Cases)
            sb.Append($" (case {c.Condition.ToString()} : {c.Stmt.ToString()})");
        
        if (DefaultClause is not null)
            sb.Append($" (default : {DefaultClause.ToString()})");

        sb.Append(" )");
        return sb.ToString();
    }

    public Expr Expr { get; } = expr;
    public IList<CaseClause> Cases { get; } = cases;
    public Stmt? DefaultClause { get; } = defaultClause;
}