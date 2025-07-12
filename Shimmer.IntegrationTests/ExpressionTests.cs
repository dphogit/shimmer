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
    
    [Fact]
    public void AddTwoNumbers_EvaluatesSum() => RunExpressionTest("1 + 2", "3");
    
    [Fact]
    public void MinusTwoNumbers_EvaluatesSubtraction() => RunExpressionTest("3 - 2", "1");

    [Fact]
    public void AddAndMinus_EvaluatesCorrectSum() => RunExpressionTest("1 + 2 - 3", "0");
}