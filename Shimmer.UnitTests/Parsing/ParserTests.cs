using Shimmer.Parsing;
using Shimmer.Parsing.Statements;
using Shimmer.Scanning;
using Shimmer.UnitTests.Helpers;

namespace Shimmer.UnitTests.Parsing;

public class ParserTests
{
    [Theory]
    [InlineData("1 + 2;", "(1 + 2)", TestDisplayName = "Addition")]
    [InlineData("1 - 2;", "(1 - 2)", TestDisplayName = "Subtraction")]
    [InlineData("1 * 2;", "(1 * 2)", TestDisplayName = "Multiplication")]
    [InlineData("1 / 2;", "(1 / 2)", TestDisplayName = "Division")]
    [InlineData("3 % 1;", "(3 % 1)", TestDisplayName = "Remainder")]
    [InlineData("1 < 2;", "(1 < 2)", TestDisplayName = "Less Than")]
    [InlineData("1 <= 2;", "(1 <= 2)", TestDisplayName = "Less Than Equal")]
    [InlineData("1 > 2;", "(1 > 2)", TestDisplayName = "Less Than")]
    [InlineData("1 >= 2;", "(1 >= 2)", TestDisplayName = "Less Than Equal")]
    [InlineData("1 == 2;", "(1 == 2)", TestDisplayName = "Equality")]
    [InlineData("1 != 2;", "(1 != 2)", TestDisplayName = "Inequality")]
    [InlineData("1 + 2 - 3;", "((1 + 2) - 3)", TestDisplayName = "Left Associativity")]
    [InlineData("6 / 3 + 1;", "((6 / 3) + 1)", TestDisplayName = "Operator Precedence")]
    [InlineData("true && true;", "(true && true)", TestDisplayName = "Logical And")]
    [InlineData("true || true;", "(true || true)", TestDisplayName = "Logical Or")]
    [InlineData("1, 2;", "(1 , 2)", TestDisplayName = "Comma")]
    [InlineData("1;", "1", TestDisplayName = "Number")]
    [InlineData("true;", "true", TestDisplayName = "True")]
    [InlineData("false;", "false", TestDisplayName = "False")]
    [InlineData("nil;", "nil", TestDisplayName = "Nil")]
    [InlineData("\"string\";", "\"string\"", TestDisplayName = "String")]
    [InlineData("\"\";", "\"\"", TestDisplayName = "Empty String")]
    [InlineData("-1;", "(- 1)", TestDisplayName = "Unary Minus")]
    [InlineData("!false;", "(! false)", TestDisplayName = "Logical Not")]
    [InlineData("(1 + 2);", "(1 + 2)", TestDisplayName = "Grouping")]
    [InlineData("true ? 1 : 2;", "(true ? 1 : 2)", TestDisplayName = "Conditional")]
    [InlineData("false ? 1 : true ? 2 : 3;", "(false ? 1 : (true ? 2 : 3))", TestDisplayName = "Nested Conditional")]
    public void Parse_ExpressionStatements_ReturnsExprStmt(string expression, string expected)
    {
        // Arrange
        var parser = new Parser(expression);

        // Act
        var stmts = parser.Parse();

        // Assert
        Assert.False(parser.HadError);
        Assert.Single(stmts);
        var exprStmt = Assert.IsType<ExprStmt>(stmts[0]);
        Assert.Equal(expected, exprStmt.ToString());
    }

    [Theory]
    [InlineData("print 1;", "(print 1)")]
    [InlineData("print 1 + 2;", "(print (1 + 2))")]
    [InlineData("print \"foo\";", "(print \"foo\")")]
    public void Parse_PrintStatement_ReturnsPrintStmt(string expression, string expected)
    {
        // Arrange
        var parser = new Parser(expression);
        
        // Act
        var stmts = parser.Parse();
        
        // Assert
        Assert.False(parser.HadError);
        Assert.Single(stmts);
        var printStmt = Assert.IsType<PrintStmt>(stmts[0]);
        Assert.Equal(expected, printStmt.ToString());
    }

    [Theory]
    [InlineData("1 + *", "[Line 1, Col 5] Error at '*': Expected expression.", TestDisplayName = "Invalid Expression")]
    [InlineData("(1 + 2", "[Line 1, Col 6] Error at end: Expected ')' after previous expression.",
        TestDisplayName = "No Closing Parenthesis")]
    [InlineData("1 + 2)", "[Line 1, Col 6] Error at ')': Expect ';' after previous expression.", TestDisplayName = "No Opening Parenthesis")]
    [InlineData("$", "[Line 1, Col 1] Error: Unexpected character '$'.", TestDisplayName = "Unexpected Character")]
    public void Parse_InvalidSyntax_ReportsError(string source, string expected)
    {
        // Arrange
        var scanner = new Scanner(source);
        var errorWriter = new StringWriter();

        var parser = new Parser(scanner, errorWriter);

        // Act
        var stmts = parser.Parse();

        // Assert
        Assert.True(parser.HadError);
        Assert.Empty(stmts);
        errorWriter.AssertOutput(expected);
    }

    [Theory]
    [InlineData("1 + *; 2 + 3;", "[Line 1, Col 5] Error at '*': Expected expression.")]
    [InlineData("/ + 2; 2 + 3;", "[Line 1, Col 1] Error at '/': Expected expression.")]
    [InlineData("1 + 2 print 2 + 3;", "[Line 1, Col 7] Error at 'print': Expect ';' after previous expression.")]
    public void Parse_ErrorInOneStatement_ReportsAndSkipsToNextStatement(string source, string expected)
    {
        // Arrange
        var errorWriter = new StringWriter();
        var parser = new Parser(source, errorWriter);
        
        // Act
        var stmts = parser.Parse();
        
        // Assert
        Assert.True(parser.HadError);
        Assert.Single(stmts);
        errorWriter.AssertOutput(expected);
    }
}