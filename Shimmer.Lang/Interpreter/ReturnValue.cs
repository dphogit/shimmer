using Shimmer.Representation;

namespace Shimmer.Interpreter;

public class ReturnValue(ShimmerValue value) : Exception
{
    public ShimmerValue Value { get; } = value;
}