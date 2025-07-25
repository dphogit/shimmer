using Shimmer.Interpreter;
using Shimmer.Parsing;
using Shimmer.Resolving;

namespace Shimmer;

public class ShimmerDriver
{
    private readonly TextWriter _stderr;

    private readonly TreeWalkInterpreter _interpreter;

    public ShimmerDriver(TextWriter? stdout = null, TextWriter? stderr = null)
    {
        stdout ??= Console.Out;
        _stderr = stderr ?? Console.Error;
        
        _interpreter = new TreeWalkInterpreter(stdout, _stderr);
    }

    /// <summary>
    /// Driver execution of the given <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source code to execute.</param>
    /// <returns>True if execution was successful, otherwise false.</returns>
    public bool Run(string source)
    {
        var parser = new Parser(source, _stderr);
        var ast = parser.Parse();

        if (parser.HadError)
            return false;

        var resolver = new Resolver(_stderr);
        var resolutionDict = resolver.Resolve(ast);

        if (resolver.HadError)
            return false;

        return _interpreter.Interpret(ast, resolutionDict);
    }

    /// <summary>
    /// Runs the given file located at <paramref name="path"/>.
    /// Fails if the file does not exist or is not a `.shim` file.
    /// </summary>
    /// <param name="path">Path of the shimmer file to execute.</param>
    /// <returns>True if running the file was successful, otherwise false.</returns>
    public bool RunFile(string path)
    {
        if (!File.Exists(path))
        {
            _stderr.WriteLine($"Error: File '{path}' not found.");
            return false;
        }

        if (Path.GetExtension(path) != ".shim")
        {
            _stderr.WriteLine($"Error: File '{Path.GetFileName(path)}' is not a .shim file.");
            return false;
        }
        
        var source = File.ReadAllText(path);
        return Run(source);
    }
}