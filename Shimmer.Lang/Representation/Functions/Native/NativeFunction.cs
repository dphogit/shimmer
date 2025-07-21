namespace Shimmer.Representation.Functions.Native;

public abstract class NativeFunction : ShimmerFunction
{
    public override string ToString() => $"<native {Name}>";
}