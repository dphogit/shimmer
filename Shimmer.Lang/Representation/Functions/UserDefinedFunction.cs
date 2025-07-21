using Shimmer.Interpreter;
using Shimmer.Parsing.Statements;
using Environment = Shimmer.Interpreter.Environment;

namespace Shimmer.Representation.Functions;

public class UserDefinedFunction(FunctionStmt declaration) : ShimmerFunction
{
    public override int Arity { get; } = declaration.Parameters.Count;
    public override string Name { get; } = declaration.Name.Lexeme;

    public override ShimmerValue Call(IInterpreter interpreter, IList<ShimmerValue> args)
    {
        var callEnvironment = BindParameters(interpreter, args);
        
        try
        {
            interpreter.ExecuteBlock(declaration.Body, callEnvironment);
        }
        catch (ReturnValue returnValue)
        {
            return returnValue.Value;
        }
        
        return ShimmerValue.Nil;
    }

    // Each function call gets its own environment, encapsulating its parameters bound to the called arguments.
    private Environment BindParameters(IInterpreter interpreter, IList<ShimmerValue> args)
    {
        if (Arity != args.Count)
            throw new InvalidOperationException($"Expected {Arity} arguments, got {args.Count}");
        
        var environment = new Environment(interpreter.Globals);

        for (var i = 0; i < Arity; i++)
            environment.Define(declaration.Parameters[i], args[i]);

        return environment;
    }

    public override string ToString() => $"<fn {Name}>";
}