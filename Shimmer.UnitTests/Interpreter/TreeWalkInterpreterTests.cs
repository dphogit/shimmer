using Shimmer.Interpreter;
using Shimmer.Parsing.Expressions;
using Shimmer.Scanning;
using Shimmer.UnitTests.Helpers;

namespace Shimmer.UnitTests.Interpreter;

public class TreeWalkInterpreterTests
{
    private readonly TokenFactory _tokenFactory = new();

    [Fact]
    public void Interpret_AddTwoNumbers_EvaluatesSum()
    {
        // Arrange
        var expr = new BinaryExpr(ExprFactory.Number(1), _tokenFactory.Plus(), ExprFactory.Number(2));
        
        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(sw);
        
        // Act
        interpreter.Interpret(expr);
        
        // Assert
        sw.AssertOutput("3");
    }
    
    [Fact]
    public void Interpret_MinusTwoNumbers_EvaluatesSubtraction()
    {
        // Arrange
        var expr = new BinaryExpr(ExprFactory.Number(3), _tokenFactory.Minus(), ExprFactory.Number(2));
        
        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(sw);
        
        // Act
        interpreter.Interpret(expr);
        
        // Assert
        sw.AssertOutput("1");
    }

    [Fact]
    public void Interpret_MultipleTerms_EvaluatesCorrectly()
    {
        // Arrange: 1 + 2 - 3 => (1 + 2) - 3
        var left = new BinaryExpr(ExprFactory.Number(1), _tokenFactory.Plus(), ExprFactory.Number(2));
        var expr = new BinaryExpr(left, _tokenFactory.Minus(), ExprFactory.Number(3));
        
        var sw = new StringWriter();
        var interpreter = new TreeWalkInterpreter(sw);
        
        // Act
        interpreter.Interpret(expr);
        
        // Assert
        sw.AssertOutput("0");
    }
    
    // TODO: Add test for unsupported operand types for binary expressions when other value types are implemented
}