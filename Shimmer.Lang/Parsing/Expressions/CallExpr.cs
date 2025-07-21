using System.Text;
using Shimmer.Scanning;

namespace Shimmer.Parsing.Expressions;

public class CallExpr(Expr callee, Token paren, List<Expr> arguments) : Expr
{
    public override string ToString()
    {
        if (Arguments.Count == 0)
            return $"(call {Callee.ToString()})";
        
        var sb = new StringBuilder($"(call {Callee.ToString()}");

        foreach (var arg in Arguments)
            sb.Append($" {arg.ToString()}");

        sb.Append(" )");
        return sb.ToString();
    }

    public Expr Callee { get; } = callee;
    public Token Paren { get; } = paren;
    public List<Expr> Arguments { get; } = arguments;
}