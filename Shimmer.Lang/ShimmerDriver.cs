using Shimmer.Interpreter;
using Shimmer.Parsing;

namespace Shimmer;

public class ShimmerDriver
{
    private readonly TextWriter _stderr;

    private readonly IInterpreter _interpreter;

    public ShimmerDriver(TextWriter? stdout = null, TextWriter? stderr = null)
    {
        stdout ??= Console.Out;
        _stderr = stderr ?? Console.Error;
        
        _interpreter = new TreeWalkInterpreter(stdout, _stderr);
    }

    /// <summary>Driver execution of the given <paramref name="source"/>.</summary>
    /// <param name="source">Source code to execute.</param>
    /// <returns>True if execution was successful, otherwise false.</returns>
    public bool Run(string source)
    {
        var parser = new Parser(source, _stderr);
        var ast = parser.Parse();

        if (parser.HadError)
            return false;

        return _interpreter.Interpret(ast);
    }
}