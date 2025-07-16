using Shimmer.Parsing;
using Shimmer.Parsing.Expressions;
using Shimmer.Scanning;
using Shimmer.UnitTests.Helpers;

namespace Shimmer.UnitTests.Parsing;

public class ParserTests
{
    [Theory]
    [InlineData("1 + 2", "(1 + 2)", TestDisplayName = "Addition")]
    [InlineData("1 - 2", "(1 - 2)", TestDisplayName = "Subtraction")]
    [InlineData("1 * 2", "(1 * 2)", TestDisplayName = "Multiplication")]
    [InlineData("1 / 2", "(1 / 2)", TestDisplayName = "Division")]
    [InlineData("3 % 1", "(3 % 1)", TestDisplayName = "Remainder")]
    [InlineData("1 < 2", "(1 < 2)", TestDisplayName = "Less Than")]
    [InlineData("1 <= 2", "(1 <= 2)", TestDisplayName = "Less Than Equal")]
    [InlineData("1 > 2", "(1 > 2)", TestDisplayName = "Less Than")]
    [InlineData("1 >= 2", "(1 >= 2)", TestDisplayName = "Less Than Equal")]
    [InlineData("1 == 2", "(1 == 2)", TestDisplayName = "Equality")]
    [InlineData("1 != 2", "(1 != 2)", TestDisplayName = "Inequality")]
    [InlineData("1 + 2 - 3", "((1 + 2) - 3)", TestDisplayName = "Left Associativity")]
    [InlineData("6 / 3 + 1", "((6 / 3) + 1)", TestDisplayName = "Operator Precedence")]
    [InlineData("true && true", "(true && true)", TestDisplayName = "Logical And")]
    [InlineData("true || true", "(true || true)", TestDisplayName = "Logical Or")]
    [InlineData("1, 2", "(1 , 2)", TestDisplayName = "Comma")]
    public void Parse_BinaryOperator_ReturnsBinaryExpr(string expression, string expected)
    {
        // Arrange
        var parser = new Parser(expression);

        // Act
        var expr = parser.Parse();

        // Assert
        var binaryExpr = Assert.IsType<BinaryExpr>(expr);
        Assert.Equal(expected, binaryExpr.ToString());
    }

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("nil")]
    public void Parse_Literal_ReturnsLiteralExpr(string source)
    {
        // Arrange
        var parser = new Parser(source);
        
        // Act
        var expr = parser.Parse();
        
        // Assert
        var literalExpr = Assert.IsType<LiteralExpr>(expr);
        Assert.Equal(source, literalExpr.ToString());
    }

    [Theory]
    [InlineData("\"string\"", "string")]
    [InlineData("\"\"", "")]
    public void Parse_String_ReturnsLiteralExpr(string source, string expected)
    {
        // Arrange
        var parser = new Parser(source);
        
        // Act
        var expr = parser.Parse();
        
        // Assert
        var literalExpr = Assert.IsType<LiteralExpr>(expr);
        Assert.Equal(expected, literalExpr.ToString());
    }

    [Theory]
    [InlineData("-1", "(- 1)")]
    [InlineData("!false", "(! false)")]
    public void Parse_UnaryOperator_ReturnsUnaryExpr(string source, string expected)
    {
        // Arrange
        var parser = new Parser(source);
        
        // Act
        var expr  = parser.Parse();
        
        // Assert
        var unaryExpr = Assert.IsType<UnaryExpr>(expr);
        Assert.Equal(expected, unaryExpr.ToString());
    }

    [Fact]
    public void Parse_Parenthesis_ReturnsGroupExpr()
    {
        // Arrange
        var parser = new Parser("(1 + 2)");

        // Act
        var expr = parser.Parse();

        // Assert
        var groupExpr = Assert.IsType<GroupExpr>(expr);
        Assert.Equal("(1 + 2)", groupExpr.ToString());
    }

    [Theory]
    [InlineData("true ? 1 : 2", "(true ? 1 : 2)")]
    [InlineData("false ? 1 : true ? 2 : 3", "(false ? 1 : (true ? 2 : 3))")]
    public void Parse_TernaryConditional_ReturnsConditionalExpr(string source, string expected)
    {
        // Arrange
        var parser = new Parser(source);
        
        // Act
        var expr = parser.Parse();
        
        // Assert
        var ternaryExpr = Assert.IsType<ConditionalExpr>(expr);
        Assert.Equal(expected, ternaryExpr.ToString());
    }

    [Theory]
    [InlineData("1 + *", "[Line 1, Col 5] Error at '*': Expected expression.", TestDisplayName = "Invalid Expression")]
    [InlineData("(1 + 2", "[Line 1, Col 6] Error at end: Expected ')' after expression.",
        TestDisplayName = "No Closing Parenthesis")]
    public void Parse_InvalidSyntax_ReturnsNullAndReportsError(string source, string expected)
    {
        // Arrange
        var scanner = new Scanner(source);
        var errorWriter = new StringWriter();

        var parser = new Parser(scanner, errorWriter);

        // Act
        var expr = parser.Parse();

        // Assert
        Assert.Null(expr);
        errorWriter.AssertOutput(expected);
    }
}