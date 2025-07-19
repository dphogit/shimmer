using Shimmer.Scanning;

namespace Shimmer.Interpreter;

public class RuntimeError : Exception
{
    private RuntimeError(string message) : base(message)
    {
    }

    public static RuntimeError Create(Token token, string message)
    {
        var error = $"[Line {token.Line}] Runtime error: {message}";
        return new RuntimeError(error);
    }
}