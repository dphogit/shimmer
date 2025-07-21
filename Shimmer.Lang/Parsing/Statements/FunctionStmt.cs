using System.Text;
using Shimmer.Scanning;

namespace Shimmer.Parsing.Statements;

public class FunctionStmt(Token name, IList<Token> parameters, BlockStmt body) : Stmt
{
    public override string ToString()
    {
        var sb = new StringBuilder($"(fn {Name.Lexeme}(");

        for (var i = 0; i < Parameters.Count; i++)
        {
            var name = Parameters[i].Lexeme;
            sb.Append(i == 0 ? name : $", {name}");
        }

        return sb.Append($") {Body.ToString()} )").ToString();
    }

    public Token Name { get; } = name;
    public IList<Token> Parameters { get; } = parameters;
    public BlockStmt Body { get; } = body;
}