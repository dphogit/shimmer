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
    public void SwitchStatement()
    {
        const string source = """
                              var x = 2;
                              
                              switch (x) {
                                case 1:  print "one";
                                case 2:  print "two";
                                default: print "other";
                              }
                              """;
        
        RunTest(source, "\"two\"");
    }

    [Fact]
    public void SwitchStatement_NoCaseMatch_ExecutesDefault()
    {
        const string source = """
                              var x = 3;
                              
                              switch (x) {
                                case 1:  print "one";
                                case 2:  print "two";
                                default: print "other";
                              }
                              """;
        
        RunTest(source, "\"other\"");
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
}