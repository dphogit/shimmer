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
    /// redirecting output to <paramref name="outputWriter" /> and errors
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
            GroupExpr groupExpr => Eval(groupExpr.Expr),
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
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber() + right.AsNumber());
            }
            case TokenType.Minus:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber() - right.AsNumber());
            }
            case TokenType.Star:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber() * right.AsNumber());
            }
            case TokenType.Slash:
            {
                CheckOperandsAreNumbers(left, right);

                if (right.AsNumber() == 0)
                    throw RuntimeError(op, "Division by 0.");

                return ShimmerValue.Number(left.AsNumber() / right.AsNumber());
            }
            case TokenType.Less:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber() < right.AsNumber());
            }
            case TokenType.LessEqual:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber() <= right.AsNumber());
            }
            case TokenType.EqualEqual:
            {
                return ShimmerValue.Bool(left.Equals(right));
            }
            case TokenType.BangEqual:
            {
                return ShimmerValue.Bool(!left.Equals(right));
            }
            case TokenType.Greater:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber() > right.AsNumber());
            }
            case TokenType.GreaterEqual:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber() >= right.AsNumber());
            }
            default:
                throw new InvalidOperationException($"Unsupported binary operator: '{op.Type}'");
        }

        void CheckOperandsAreNumbers(ShimmerValue leftValue, ShimmerValue rightValue)
        {
            if (!leftValue.IsNumber || !rightValue.IsNumber)
                throw UnsupportedOperandTypeBinary(op, leftValue.Type, rightValue.Type);
        }
    }

    private static ShimmerValue EvalLiteralExpr(LiteralExpr literalExpr) => literalExpr.Value;

    private static RuntimeException RuntimeError(Token token, string message)
    {
        var error = $"[Line {token.Line}] Runtime error: {message}";
        return new RuntimeException(error);
    }

    private static RuntimeException UnsupportedOperandTypeBinary(Token op, ShimmerType left, ShimmerType right)
    {
        var exception = RuntimeError(op, $"Unsupported operand type(s) for '{op.Lexeme}': '{left}' and '{right}'.");
        return new RuntimeException(exception.Message);
    }
}