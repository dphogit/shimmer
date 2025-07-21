using Shimmer.UnitTests.Helpers;

namespace Shimmer.IntegrationTests;

public class FileTests
{
    private static readonly string ProgramsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ShimmerPrograms");
    
    [Fact]
    public void RunFile_Existing_PrintsFirstTenFibonacciNumbers()
    {
        // Arrange
        var path = Path.Combine(ProgramsDirectory, "fib.shim");
        var expectedOutput = string.Join(Environment.NewLine, ["0", "1", "1", "2", "3", "5", "8", "13", "21", "34"]);

        var output = new StringWriter();
        var driver = new ShimmerDriver(output);
        
        // Act
        var success = driver.RunFile(path);

        // Assert
        Assert.True(success);
        output.AssertOutput(expectedOutput);
    }

    [Fact]
    public void RunFile_DoesNotExist_GivesError()
    {
        // Arrange
        var path = Path.Combine(ProgramsDirectory, "bogey.shim");

        var error = new StringWriter();
        var driver = new ShimmerDriver(stderr: error);
        
        // Act
        var success = driver.RunFile(path);
        
        // Assert
        Assert.False(success);
        error.AssertOutput($"Error: File '{path}' not found.");
    }

    [Fact]
    public void RunFile_DoesNotEndInShimExtension_GivesError()
    {
        // Arrange
        var path = Path.Combine(ProgramsDirectory, "fib");
        
        var error = new StringWriter();
        var driver = new ShimmerDriver(stderr: error);
        
        // Act
        var success = driver.RunFile(path);
        
        // Assert
        Assert.False(success);
        error.AssertOutput($"Error: File 'fib' is not a .shim file.");
    }
}