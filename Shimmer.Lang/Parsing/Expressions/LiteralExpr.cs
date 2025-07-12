using Shimmer.Representation;

namespace Shimmer.Parsing.Expressions;

public class LiteralExpr(ShimmerValue value) : Expr
{
    public override string ToString() => Value.ToString()!;

    public ShimmerValue Value { get; } = value;
}