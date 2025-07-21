using Shimmer.Interpreter;

namespace Shimmer.Representation.Functions.Native;

public class TypeOfFunction : NativeFunction 
{
    public override int Arity => 1;
    public override string Name => "typeof";
    
    public override ShimmerValue Call(IInterpreter interpreter, IList<ShimmerValue> args) =>
        ShimmerValue.String(args[0].Type.ToString());
}