using Shimmer.Interpreter;
using Shimmer.Parsing.Expressions;
using Shimmer.Scanning;
using Shimmer.UnitTests.Helpers;

namespace Shimmer.UnitTests.Interpreter;

public class TreeWalkInterpreterTests
{
    private readonly TokenFactory _tokenFactory = new();

    [Theory]
    [InlineData(1, "+", 2, "3", TestDisplayName = "Addition")]
    [InlineData(3, "-", 2, "1", TestDisplayName = "Subtraction")]
    [InlineData(1, "*", 2, "2", TestDisplayName = "Multiplication")]
    [InlineData(4, "/", 2, "2", TestDisplayName = "Division")]
    public void Interpret_Arithmetic_EvaluatesCorrectly(double a, string op, double b, string expected)
    {
        // Arrange
        var expr = new BinaryExpr(ExprFactory.Number(a), _tokenFactory.Create(op), ExprFactory.Number(b));

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(sw);

        // Act
        interpreter.Interpret(expr);

        // Assert
        sw.AssertOutput(expected);
    }

    [Theory]
    [InlineData(1, "<", 2, "true")]
    [InlineData(2, "<", 2, "false")]
    [InlineData(3, "<", 2, "false")]
    [InlineData(1, "<=", 2, "true")]
    [InlineData(2, "<=", 2, "true")]
    [InlineData(3, "<=", 2, "false")]
    [InlineData(1, ">", 2, "false")]
    [InlineData(2, ">", 2, "false")]
    [InlineData(3, ">", 2, "true")]
    [InlineData(1, ">=", 2, "false")]
    [InlineData(2, ">=", 2, "true")]
    [InlineData(3, ">=", 2, "true")]
    [InlineData(1, "==", 1, "true")]
    [InlineData(1, "==", 2, "false")]
    [InlineData(2, "==", 1, "false")]
    [InlineData(1, "!=", 1, "false")]
    [InlineData(2, "!=", 1, "true")]
    [InlineData(1, "!=", 2, "true")]
    public void Interpret_Relational_EvaluatesCorrectly(double a, string op, double b, string expected)
    {
        // Arrange
        var expr = new BinaryExpr(ExprFactory.Number(a), _tokenFactory.Create(op), ExprFactory.Number(b));

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(sw);

        // Act
        interpreter.Interpret(expr);

        // Assert
        sw.AssertOutput(expected);
    }

    [Fact]
    public void Interpret_DivideByZero_GivesRuntimeException()
    {
        // Arrange
        var expr = new BinaryExpr(ExprFactory.Number(1), _tokenFactory.Slash(), ExprFactory.Number(0));

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);

        // Act
        interpreter.Interpret(expr);

        // Assert
        sw.AssertRuntimeError(1, "Division by 0.");
    }

    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("*")]
    [InlineData("/")]
    [InlineData("<")]
    [InlineData("<=")]
    [InlineData(">=")]
    [InlineData(">")]
    public void Interpret_IllegalOperandTypes_GivesRuntimeException(string op)
    {
        // Arrange
        var opToken = _tokenFactory.Create(op);
        var expr = new BinaryExpr(ExprFactory.Number(1), opToken, LiteralExpr.True);
        
        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);
        
        // Act
        interpreter.Interpret(expr);
        
        // Assert
        sw.AssertRuntimeError(1, $"Unsupported operand type(s) for '{opToken.Lexeme}': 'Number' and 'Bool'.");
    }
}