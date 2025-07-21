using Shimmer.Parsing.Expressions;
using Shimmer.Scanning;

namespace Shimmer.Parsing.Statements;

public class ReturnStmt(Token keyword, Expr expr) : Stmt
{
    public override string ToString() => $"(return {Expr.ToString()})";

    public Token Keyword { get; } = keyword;
    public Expr Expr { get; }  = expr;
}