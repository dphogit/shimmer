using Shimmer.Interpreter;
using Shimmer.Parsing.Expressions;
using Shimmer.Parsing.Statements;
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
    [InlineData(13, "%", 5, "3", TestDisplayName = "Remainder")]
    public void Interpret_Arithmetic_EvaluatesCorrectly(double a, string op, double b, string expected)
    {
        // Arrange
        var expr = new BinaryExpr(ExprFactory.Number(a), _tokenFactory.Create(op), ExprFactory.Number(b));
        var stmt = new PrintStmt(expr);

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(sw);

        // Act
        var success = interpreter.Interpret([stmt]);

        // Assert
        Assert.True(success);
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
        var stmt = new PrintStmt(expr);

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(sw);

        // Act
        var success = interpreter.Interpret([stmt]);

        // Assert
        Assert.True(success);
        sw.AssertOutput(expected);
    }

    [Fact]
    public void Interpret_DivideByZero_GivesRuntimeError()
    {
        // Arrange
        var expr = new BinaryExpr(ExprFactory.Number(1), _tokenFactory.Slash(), ExprFactory.Number(0));
        var stmt = new PrintStmt(expr);

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);

        // Act
        var success = interpreter.Interpret([stmt]);

        // Assert
        Assert.False(success);
        sw.AssertRuntimeError(1, "Division by 0.");
    }

    [Fact]
    public void Interpret_NegationOperandNotNumber_GivesRuntimeError()
    {
        // Arrange
        var expr = new UnaryExpr(_tokenFactory.Minus(), LiteralExpr.True);
        var stmt = new PrintStmt(expr);

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);

        // Act
        var success = interpreter.Interpret([stmt]);

        // Assert
        Assert.False(success);
        sw.AssertRuntimeError(1, $"Bad operand type for unary '-': 'Bool'.");
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
    public void Interpret_IllegalBinaryOperands_GivesRuntimeError(string op)
    {
        // Arrange
        var opToken = _tokenFactory.Create(op);
        var expr = new BinaryExpr(ExprFactory.Number(1), opToken, LiteralExpr.True);
        var stmt = new PrintStmt(expr);

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);

        // Act
        var success = interpreter.Interpret([stmt]);

        // Assert
        Assert.False(success);
        sw.AssertRuntimeError(1, $"Unsupported operand type(s) for '{opToken.Lexeme}': 'Number' and 'Bool'.");
    }

    [Fact]
    public void Interpret_AddNumberAndString_GivesRuntimeError()
    {
        // Arrange
        var plus = _tokenFactory.Create("+");
        var expr = new BinaryExpr(ExprFactory.Number(1), plus, ExprFactory.String("1"));
        var stmt = new PrintStmt(expr);

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);

        // Act
        var success = interpreter.Interpret([stmt]);

        // Assert
        Assert.False(success);
        sw.AssertRuntimeError(1, $"Unsupported operand type(s) for '+': 'Number' and 'String'.");
    }

    [Fact]
    public void Interpret_AddStringAndNumber_GivesRuntimeError()
    {
        // Arrange
        var plus = _tokenFactory.Create("+");
        var expr = new BinaryExpr(ExprFactory.String("1"), plus, ExprFactory.Number(1));
        var stmt = new PrintStmt(expr);

        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);

        // Act
        var success = interpreter.Interpret([stmt]);

        // Assert
        Assert.False(success);
        sw.AssertRuntimeError(1, $"Unsupported operand type(s) for '+': 'String' and 'Number'.");
    }

    [Fact]
    public void Interpret_VariableWithSameNameDefinedInSameScope_GivesRuntimeError()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        var varDecl1 = new VarStmt(name);
        var varDecl2 = new VarStmt(name);
        
        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);
        
        // Act
        var success = interpreter.Interpret([varDecl1, varDecl2]);
        
        // Assert
        Assert.False(success);
        sw.AssertRuntimeError(1, $"Variable '{name.Lexeme}' already defined in this scope.");
    }

    [Fact]
    public void Interpret_UndefinedVariable_GivesRuntimeError()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        var stmt = new ExprStmt(new VarExpr(name));
        
        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(errorWriter: sw);
        
        // Act
        var success = interpreter.Interpret([stmt]);
        
        // Assert
        Assert.False(success);
        sw.AssertRuntimeError(1, $"Undefined variable '{name.Lexeme}'.");
    }
}