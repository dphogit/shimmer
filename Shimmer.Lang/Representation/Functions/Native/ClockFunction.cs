using Shimmer.Interpreter;

namespace Shimmer.Representation.Functions.Native;

public class ClockFunction : NativeFunction
{
    public override int Arity => 0;
    public override string Name => "clock";

    public override ShimmerValue Call(IInterpreter interpreter, IList<ShimmerValue> args) =>
        ShimmerValue.Number(DateTimeOffset.Now.ToUnixTimeMilliseconds());
}