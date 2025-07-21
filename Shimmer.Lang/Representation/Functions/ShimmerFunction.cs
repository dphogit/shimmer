using Shimmer.Interpreter;

namespace Shimmer.Representation.Functions;

public abstract class ShimmerFunction
{
    public abstract int Arity { get; }
    public abstract string Name { get; }

    public abstract ShimmerValue Call(IInterpreter interpreter, IList<ShimmerValue> args); 
}