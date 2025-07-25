using Shimmer.Parsing.Statements;

namespace Shimmer.Interpreter;

public interface IInterpreter
{
    /// <summary>
    /// Executes the block statement using the given <paramref name="environment"/>.
    /// </summary>
    /// <param name="blockStmt">The block statement containing the list of statements to execute.</param>
    /// <param name="environment">
    /// The interpreter will set its current execution environment to this when executing the block.
    /// </param>
    public void ExecuteBlock(BlockStmt blockStmt, Environment environment);
}