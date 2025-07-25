using Shimmer.Scanning;

namespace Shimmer.Parsing.Statements;

public class BreakStmt(Token keyword) : Stmt
{
    public override string ToString() => "break";
    
    public Token Keyword { get; } = keyword;
}