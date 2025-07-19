using Shimmer.UnitTests.Helpers;

namespace Shimmer.IntegrationTests;

public class BaseIntegrationTest
{
    protected static void RunTest(string source, string expected)
    {
        // Arrange
        var output = new StringWriter();
        var driver = new ShimmerDriver(output);
        
        // Act
        var success = driver.Run(source);
        
        // Assert
        Assert.True(success);
        output.AssertOutput(expected);
    }

    protected static void RunErrorTest(string source, string expected)
    {
        // Arrange
        var error = new StringWriter();
        var driver = new ShimmerDriver(stderr: error);
        
        // Act
        var result = driver.Run(source);
        
        // Assert
        Assert.False(result);
        error.AssertOutput(expected);
    }
}