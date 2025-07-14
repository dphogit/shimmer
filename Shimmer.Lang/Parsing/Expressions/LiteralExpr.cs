using Shimmer.Representation;

namespace Shimmer.Parsing.Expressions;

public class LiteralExpr(ShimmerValue value) : Expr
{
    public static readonly LiteralExpr True = new(ShimmerValue.True);
    public static readonly LiteralExpr False = new(ShimmerValue.False);
    public static readonly LiteralExpr Nil = new(ShimmerValue.Nil);
    
    public override string ToString() => Value.ToString()!;

    public ShimmerValue Value { get; } = value;
}