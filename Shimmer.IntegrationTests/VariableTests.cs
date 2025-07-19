namespace Shimmer.IntegrationTests;

public class VariableTests : BaseIntegrationTest
{
    [Theory]
    [InlineData("var x = 10; print x;", "10", TestDisplayName = "Define Variable")]
    [InlineData("var x = 10; var y = 3; print x % y;", "1", TestDisplayName = "Multiple Variables")]
    [InlineData("var a = 1; var b = a + 2; print b;", "3", TestDisplayName = "Reference Other Variables")]
    [InlineData("var x; print x = 5;", "5", TestDisplayName = "Assignment")]
    [InlineData("var x; var y = x = 20; print x + y;", "40", TestDisplayName = "Right Associative Assignment")]
    [InlineData("var x; var y; x = 1, y = 2; print x + y;", "3", TestDisplayName = "Assign Multiple Same Line")]
    public void Usage(string source, string expected) => RunTest(source, expected);

    [Fact]
    public void ShadowGlobal()
    {
        // Arrange
        const string source = """
                              var x = "outer";
                              {
                                var x = "inner";
                                print x;
                              }
                              print x;
                              """;
        
        var expected = $"\"inner\"{Environment.NewLine}\"outer\"";
        
        RunTest(source, expected);
    }

    [Fact]
    public void ShadowLocal()
    {
        const string source = """
                              {
                                var x = "local";
                                {
                                  var x = "shadow";
                                  print x;
                                }
                                print x;
                              }
                              """;

        var expected = $"\"shadow\"{Environment.NewLine}\"local\"";
        
        RunTest(source, expected);
    }
}