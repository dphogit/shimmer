using Shimmer.Scanning;

namespace Shimmer.Parsing.Expressions;

public class BinaryExpr(Expr left, Token op, Expr right) : Expr
{
    public override string ToString() => $"({left.ToString()} {op.Lexeme} {right.ToString()})";
    
    public Expr Left => left;
    public Expr Right => right;
    public Token Op => op;
}