using System.Diagnostics;
using System.Globalization;

namespace Shimmer.Representation;

public class ShimmerValue
{
    public ShimmerType Type { get; }
    private object? Value { get; }

    private ShimmerValue(ShimmerType type, object? value)
    {
        Type = type;
        Value = value;
    }

    public static ShimmerValue Number(double d) => new(ShimmerType.Number, d);
    public bool IsNumber => Type == ShimmerType.Number;
    public double AsNumber() =>
        IsNumber ? (double)Value! : throw new InvalidOperationException("Value is not a number.");

    public override string ToString() =>
        Type switch
        {
            ShimmerType.Number => AsNumber().ToString(CultureInfo.InvariantCulture)!,
            _ => throw new UnreachableException($"Unknown shimmer value type '{Type}'."),
        };
}