using Shimmer.Parsing.Expressions;

namespace Shimmer.Interpreter;

public interface IInterpreter
{
    public void Interpret(Expr expr);
}