using Shimmer.Representation;

namespace Shimmer.Parsing.Expressions;

public static class ExprFactory
{
    public static LiteralExpr Number(double d) => new(ShimmerValue.Number(d));
}