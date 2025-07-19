namespace Shimmer.IntegrationTests;

public class ControlFlowTests : BaseIntegrationTest
{
    [Theory]
    [InlineData("if (true) print 1;", "1")]
    [InlineData("if (true) print 1; else print 2;", "1")]
    [InlineData("if (false) print 1;", "")]
    [InlineData("if (false) print 1; else print 2;", "2")]
    [InlineData("var x = 1; if (true) { x = 2; } print x;", "2")]
    [InlineData("var x = 1; if (false) { x = 2; } print x;", "1")]
    public void IfStatements(string source, string expected) => RunTest(source, expected);

    [Fact]
    public void WhileLoop()
    {
        // Arrange
        const string source = """
                              var i = 0;
                              var sum = 0;
                              
                              while (i < 3) {
                                i = i + 1;
                                sum = sum + i;
                              }
                              
                              print sum;
                              """;
        
        RunTest(source, "6");
    }

    [Fact]
    public void ForLoop()
    {
        const string source = """
                              var sum = 0;
                              
                              for (var i = 0; i < 3; i = i + 1) {
                                sum = sum + i + 1;
                              }
                              
                              print sum;
                              """;
        
        RunTest(source, "6");
    }

    [Fact]
    public void DoWhileLoop()
    {
        const string source = """
                              var i = 1;
                              var sum = 0;
                              
                              do {
                                sum = sum + i;
                                i = i + 1;
                              } while (i <= 3);
                              
                              print sum;
                              """;
        
        RunTest(source, "6");
    }

    [Fact]
    public void Break_ExitsLoop()
    {
        const string source = """
                              var sum = 0;
                              
                              for (var i = 1; i < 10; i = i + 1) {
                                if (i == 5)
                                    break;
                                sum = sum + i; 
                              }
                              
                              print sum;
                              """;

        RunTest(source, "10");
    }

    [Fact]
    public void Continue_SkipsToNextIteration()
    {
        const string source = """
                              var sum = 0;
                              
                              for (var i = 1; i < 10; i = i + 1) {
                                if (i % 2 == 0)
                                    continue;
                                sum = sum + i;
                              }
                              
                              print sum;
                              """;
        
        RunTest(source, "25");
    }
    
    [Fact]
    public void BreakOutsideLoop_GivesError()
    {
        const string source = "break;";
        const string expected = "[Line 1, Col 1] Error at 'break': Must be inside a loop to break.";
        RunErrorTest(source, expected);
    }

    [Fact]
    public void ContinueOutsideLoop_GivesError()
    {
        const string source = "continue;";
        const string expected = "[Line 1, Col 1] Error at 'continue': Must be inside a loop to continue.";
        RunErrorTest(source, expected);
    }
}