using System.Text;

namespace Shimmer.Parsing.Statements;

public class BlockStmt(IList<Stmt> statements) : Stmt
{
    public override string ToString()
    {
        var sb = new StringBuilder("{ ");

        foreach (var stmt in Statements)
            sb.Append($"{stmt.ToString()} ");

        sb.Append('}');
        return sb.ToString();
    }

    public IList<Stmt> Statements { get; } = new List<Stmt>(statements);
}