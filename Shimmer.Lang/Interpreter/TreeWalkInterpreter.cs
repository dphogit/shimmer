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

    private ShimmerValue Eval(Expr expr) => expr switch
    {
        BinaryExpr binaryExpr => EvalBinaryExpr(binaryExpr),
        GroupExpr groupExpr => Eval(groupExpr.Expr),
        LiteralExpr literalExpr => literalExpr.Value,
        UnaryExpr unaryExpr => EvalUnaryExpr(unaryExpr),
        _ => throw new UnreachableException($"Unknown expression type '{expr.GetType().Name}'")
    };

    private ShimmerValue EvalBinaryExpr(BinaryExpr binaryExpr)
    {
        var op = binaryExpr.Op;

        // First check for logical operators because we don't want to evaluate
        // both operands as these operations can short-circuit on the first operand.
        switch (op.Type)
        {
            case TokenType.And:
            {
                var leftAnd = Eval(binaryExpr.Left);
                return IsFalsy(leftAnd) ? leftAnd : Eval(binaryExpr.Right);
            }
            case TokenType.Or:
            {
                var leftOr = Eval(binaryExpr.Left);
                return !IsFalsy(leftOr) ? leftOr : Eval(binaryExpr.Right);
            }
        }

        ShimmerValue left = Eval(binaryExpr.Left), right = Eval(binaryExpr.Right);

        switch (op.Type)
        {
            case TokenType.Plus:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber + right.AsNumber);
            }
            case TokenType.Minus:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber - right.AsNumber);
            }
            case TokenType.Star:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber * right.AsNumber);
            }
            case TokenType.Slash:
            {
                CheckOperandsAreNumbers(left, right);

                if (right.AsNumber == 0)
                    throw RuntimeError(op, "Division by 0.");

                return ShimmerValue.Number(left.AsNumber / right.AsNumber);
            }
            case TokenType.Less:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber < right.AsNumber);
            }
            case TokenType.LessEqual:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber <= right.AsNumber);
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
                return ShimmerValue.Bool(left.AsNumber > right.AsNumber);
            }
            case TokenType.GreaterEqual:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber >= right.AsNumber);
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

    private ShimmerValue EvalUnaryExpr(UnaryExpr unaryExpr)
    {
        var op = unaryExpr.Op;
        var operand = Eval(unaryExpr.Expr);

        return op.Type switch
        {
            TokenType.Minus => operand.IsNumber
                ? ShimmerValue.Number(-operand.AsNumber)
                : throw UnsupportedOperandTypeUnary(op, operand.Type),

            TokenType.Bang => IsFalsy(operand) ? ShimmerValue.True : ShimmerValue.False,

            _ => throw new InvalidOperationException($"Unsupported unary operator: '{op.Type}'")
        };
    }

    private static bool IsFalsy(ShimmerValue value) => value.IsNil || value is { IsBool: true, AsBool: false };

    private static RuntimeException RuntimeError(Token token, string message)
    {
        var error = $"[Line {token.Line}] Runtime error: {message}";
        return new RuntimeException(error);
    }

    private static RuntimeException UnsupportedOperandTypeUnary(Token op, ShimmerType argType) =>
        RuntimeError(op, $"Bad operand type for unary '{op.Lexeme}': '{argType}'.");

    private static RuntimeException UnsupportedOperandTypeBinary(Token op, ShimmerType left, ShimmerType right) =>
        RuntimeError(op, $"Unsupported operand type(s) for '{op.Lexeme}': '{left}' and '{right}'.");
}