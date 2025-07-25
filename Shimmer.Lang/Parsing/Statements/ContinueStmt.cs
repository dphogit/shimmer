using Shimmer.Scanning;

namespace Shimmer.Parsing.Statements;

public class ContinueStmt(Token keyword) : Stmt
{
    public override string ToString() => "continue";
    
    public Token Keyword { get; } = keyword;
}