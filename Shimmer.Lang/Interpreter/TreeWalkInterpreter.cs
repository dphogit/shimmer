using System.Diagnostics;
using Shimmer.Parsing.Expressions;
using Shimmer.Representation;
using Shimmer.Scanning;

namespace Shimmer.Interpreter;

public class TreeWalkInterpreter : IInterpreter
{
    private class RuntimeException(string message) : Exception(message);

    private readonly TextWriter _outputWriter;
    private readonly TextWriter _errorWriter;


    /// <summary>
    /// The interpreter walks the given AST in its <see cref="Interpret"/> method,
    /// redirecting stdout to the given <paramref name="outputWriter" /> and errors
    /// to <paramref name="errorWriter"/>
    /// </summary>
    /// <param name="outputWriter">Defaults to <see cref="Console.Out"/>.</param>
    /// <param name="errorWriter">Defaults to <see cref="Console.Error"/>.</param>
    public TreeWalkInterpreter(TextWriter? outputWriter = null, TextWriter? errorWriter = null)
    {
        _outputWriter = outputWriter ?? Console.Out;
        _errorWriter = errorWriter ?? Console.Error;
    }

    public void Interpret(Expr expr)
    {
        try
        {
            var value = Eval(expr);
            _outputWriter.WriteLine(value);
        }
        catch (RuntimeException e)
        {
            _errorWriter.WriteLine(e.Message);
        }
    }

    private ShimmerValue Eval(Expr expr)
    {
        return expr switch
        {
            BinaryExpr binaryExpr => EvalBinaryExpr(binaryExpr),
            LiteralExpr literalExpr => EvalLiteralExpr(literalExpr),
            _ => throw new UnreachableException($"Unknown expression type '{expr.GetType().Name}'")
        };
    }

    private ShimmerValue EvalBinaryExpr(BinaryExpr binaryExpr)
    {
        ShimmerValue left = Eval(binaryExpr.Left), right = Eval(binaryExpr.Right);
        var op = binaryExpr.Op;

        switch (op.Type)
        {
            case TokenType.Plus:
            {
                if (left.IsNumber && right.IsNumber)
                    return ShimmerValue.Number(left.AsNumber() + right.AsNumber());

                throw UnsupportedOperandTypeBinary(op, left.Type, right.Type);
            }
            case TokenType.Minus:
            {
                if (left.IsNumber && right.IsNumber)
                    return ShimmerValue.Number(left.AsNumber() - right.AsNumber());
                throw UnsupportedOperandTypeBinary(op, left.Type, right.Type);
            }
            default:
                throw new InvalidOperationException($"Unsupported binary operator: '{op.Type}'");
        }
    }

    private static ShimmerValue EvalLiteralExpr(LiteralExpr literalExpr) => literalExpr.Value;

    private static RuntimeException RuntimeError(Token token, string message)
    {
        var error = $"[Line {token.Line}] Runtime Error: {message}";
        return new RuntimeException(error);
    }

    private static RuntimeException UnsupportedOperandTypeBinary(Token op, ShimmerType left, ShimmerType right)
    {
        var exception = RuntimeError(op, $"Unsupported operand type(s) for '{op.Lexeme}': '{left}' and '{right}'.");
        return new RuntimeException(exception.Message);
    }
}