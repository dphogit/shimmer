using Moq;
using Shimmer.Errors;
using Shimmer.Parsing;
using Shimmer.Parsing.Expressions;
using Shimmer.Scanning;

namespace Shimmer.UnitTests.Parsing;

public class ParserTests
{
    [Fact]
    public void Parse_Add_ReturnsExpectedBinaryExpr()
    {
        // Arrange
        var parser = new Parser("1 + 2");
        
        // Act
        var expr = parser.Parse();
        
        // Assert
        var binaryExpr = Assert.IsType<BinaryExpr>(expr);
        Assert.Equal("(1 + 2)", binaryExpr.ToString());
    }

    [Fact]
    public void Parse_LeftAssociative_ReturnsExpectedBinaryExpr()
    {
        // Arrange
        var parser = new Parser("1 + 2 - 3");
        
        // Act
        var expr = parser.Parse();
        
        // Assert
        var binaryExpr = Assert.IsType<BinaryExpr>(expr);
        Assert.Equal("((1 + 2) - 3)", binaryExpr.ToString());
    }

    [Fact]
    public void Parse_InvalidExpression_ReturnsNullAndReportsError()
    {
        // Arrange
        var scanner = new Scanner("1 + -");
        var errorReporterMock = new Mock<IErrorReporter>();
        
        var parser = new Parser(scanner, errorReporterMock.Object);
        
        // Act
        var expr = parser.Parse();
        
        // Assert
        Assert.Null(expr);
        errorReporterMock.Verify(r => r.ReportError("[Line 1, Col 5] Error at '-': Expected expression."));
    }
}