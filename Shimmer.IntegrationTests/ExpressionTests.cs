using Shimmer.UnitTests.Helpers;

namespace Shimmer.IntegrationTests;

public class ExpressionTests
{
    private static void RunExpressionTest(string expression, string expected)
    {
        var output = new StringWriter();
        var driver = new ShimmerDriver(output);
        
        var success = driver.Run(expression);
        
        Assert.True(success);
        output.AssertOutput(expected);
    }
    
    [Theory]
    [InlineData("1 + 2", "3", TestDisplayName = "Addition")]
    [InlineData("3 - 2", "1", TestDisplayName = "Subtraction")]
    [InlineData("4 * 2", "8", TestDisplayName = "Multiplication")]
    [InlineData("4 / 2", "2", TestDisplayName = "Division")]
    [InlineData("1 + 2 - 3", "0", TestDisplayName = "Left Associativity")]
    [InlineData("6 + 9 / 3", "9", TestDisplayName = "Operator Precedence")]
    [InlineData("(2 + 3) * 4", "20", TestDisplayName = "Grouping Parenthesis")]
    public void Arithmetic_EvaluatesCorrectly(string expression, string expected) =>
        RunExpressionTest(expression, expected);

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
    [InlineData("nil == nil", TestDisplayName = "Nil Equals Nil")]
    [InlineData("nil != 3", TestDisplayName = "Nil Equality Different Type")]
    public void Relational_EvaluatesTrue(string expression) =>
        RunExpressionTest(expression, "true");

    [Theory]
    [InlineData("true || false", "true", TestDisplayName = "Logical Or Short Circuit")]
    [InlineData("1 || false", "1", TestDisplayName = "Logical Or Truthy First Operand")]
    [InlineData("false || nil", "nil", TestDisplayName = "Logical Or Falsy First Operand")]
    [InlineData("false && true", "false", TestDisplayName = "Logical And Short Circuit")]
    [InlineData("nil && 1", "nil", TestDisplayName = "Logical And Falsy First Operand")]
    [InlineData("true && 2", "2", TestDisplayName = "Logical And Both True")]
    [InlineData("true && true && nil", "nil", TestDisplayName = "Multiple Logical And")]
    [InlineData("false || nil || 1", "1", TestDisplayName = "Multiple Logical Or")]
    public void Logical_EvaluatesCorrectly(string expression, string expected) =>
        RunExpressionTest(expression, expected);

    [Theory]
    [InlineData("!false", "true", TestDisplayName = "Logical Not False")]
    [InlineData("!true", "false", TestDisplayName = "Logical Not True")]
    [InlineData("!!true", "true", TestDisplayName = "Double Logical Not")]
    [InlineData("!!!true", "false", TestDisplayName = "Triple Logical Not")]
    [InlineData("!nil", "true", TestDisplayName = "Logical Not Falsy Value")]
    [InlineData("-1", "-1", TestDisplayName = "Negation")]
    [InlineData("--1", "1", TestDisplayName = "Double Negation")]
    [InlineData("---1", "-1", TestDisplayName = "Triple Negation")]
    public void Unary_EvaluatesCorrectly(string expression, string expected) =>
        RunExpressionTest(expression, expected);

    [Theory]
    [InlineData("1, 2", "2", TestDisplayName = "Two Elements")]
    [InlineData("1 + 2, 3 + 4", "7", TestDisplayName = "Subexpressions")]
    [InlineData("1, 2, 4", "4", TestDisplayName = "Multiple Elements")]
    public void Comma_EvaluatesToLastExpression(string expression, string expected) =>
        RunExpressionTest(expression, expected);

    [Theory]
    [InlineData("true ? 1 : 2", "1", TestDisplayName = "True Condition")]
    [InlineData("false ? 1 : 2", "2", TestDisplayName = "False Condition")]
    [InlineData("1 ? 2 : 2", "2", TestDisplayName = "Truthy Condition")]
    [InlineData("nil ? 1 : 2", "2", TestDisplayName = "Falsy Condition")]
    [InlineData("false ? 1 : true ? 2 : 3", "2", TestDisplayName = "Right Associativity")]
    public void Conditional_EvaluatesCorrectly(string expression, string expected) =>
        RunExpressionTest(expression, expected);
}