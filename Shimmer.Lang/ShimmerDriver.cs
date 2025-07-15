using Shimmer.Interpreter;
using Shimmer.Parsing;

namespace Shimmer;

public class ShimmerDriver(TextWriter? stdout = null, TextWriter? stderr = null)
{
    private readonly TextWriter _stdout = stdout ?? Console.Out;
    private readonly TextWriter _stderr = stderr ?? Console.Error;

    /// <summary>Driver execution of the given <paramref name="source"/>.</summary>
    /// <param name="source">Source code to execute.</param>
    /// <returns>True if execution was successful, otherwise false.</returns>
    public bool Run(string source)
    {
        var parser = new Parser(source, _stderr);
        var ast = parser.Parse();

        if (ast is null)
            return false;

        var interpreter = new TreeWalkInterpreter(_stdout, _stderr);
        interpreter.Interpret(ast);

        return true;
    }
}