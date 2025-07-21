using Shimmer.Parsing.Statements;

namespace Shimmer.Interpreter;

public interface IInterpreter
{
    /// <summary>
    /// Executes the program given as a list of statements.
    /// </summary>
    /// <param name="stmts">The list of statements to execute.</param>
    /// <returns>True if successful, otherwise false.</returns>
    public bool Interpret(IList<Stmt> stmts);

    public Environment Globals { get; }
    
    /// <summary>
    /// Executes the given block statement within the <paramref name="environment"/>.
    /// </summary>
    /// <param name="blockStmt">The block statement containing the list of statements to execute.</param>
    /// <param name="environment">
    /// The interpreter will set its current execution environment to this when executing the block.
    /// </param>
    public void ExecuteBlock(BlockStmt blockStmt, Environment environment);
}