using Shimmer.Parsing.Expressions;
using Shimmer.Scanning;

namespace Shimmer.Parsing.Statements;

public class VarStmt(Token name, Expr? initializer = null) : Stmt
{
    public override string ToString() => $"(var {Name.Lexeme} = {Initializer.ToString()})";
    
    public Token Name { get; } = name;
    public Expr Initializer { get; } = initializer ?? LiteralExpr.Nil;
}