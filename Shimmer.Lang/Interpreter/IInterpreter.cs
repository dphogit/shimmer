using Shimmer.Parsing.Statements;

namespace Shimmer.Interpreter;

public interface IInterpreter
{
    public void Interpret(IList<Stmt> stmts);
}