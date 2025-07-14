using Shimmer.UnitTests.Helpers;

namespace Shimmer.IntegrationTests;

public class ExpressionTests
{
    [Theory]
    [InlineData("1 + 2", "3", TestDisplayName = "Addition")]
    [InlineData("3 - 2", "1", TestDisplayName = "Subtraction")]
    [InlineData("4 * 2", "8", TestDisplayName = "Multiplication")]
    [InlineData("4 / 2", "2", TestDisplayName = "Division")]
    [InlineData("1 + 2 - 3", "0", TestDisplayName = "Left Associativity")]
    [InlineData("6 + 9 / 3", "9", TestDisplayName = "Operator Precedence")]
    [InlineData("(2 + 3) * 4", "20", TestDisplayName = "Grouping Parenthesis")]
    public void Arithmetic_EvaluatesCorrectly(string expression, string expected)
    {
        var output = new StringWriter();
        var driver = new ShimmerDriver(output);

        var success = driver.Run(expression);

        Assert.True(success);
        output.AssertOutput(expected);
    }

    [Theory]
    [InlineData("1 < 2", TestDisplayName = "Less Than")]
    [InlineData("1 <= 2", TestDisplayName = "Less Equal Than")]
    [InlineData("3 > 2", TestDisplayName = "Greater Than")]
    [InlineData("3 >= 2", TestDisplayName = "Greater Equal Than")]
    [InlineData("1 == 1", TestDisplayName = "Equal Equal")]
    [InlineData("1 != 2", TestDisplayName = "Not Equal")]
    [InlineData("1 < 2 == 2 > 1", TestDisplayName = "Operator Precedence")]
    [InlineData("true == true", TestDisplayName = "Boolean Equality")]
    [InlineData("false != true", TestDisplayName = "Boolean Inequality")]
    public void Relational_EvaluatesTrue(string expression)
    {
        var output = new StringWriter();
        var driver = new ShimmerDriver(output);

        var success = driver.Run(expression);

        Assert.True(success);
        output.AssertOutput("true");
    }
}