namespace Shimmer.IntegrationTests;

public class ClosureTests : BaseIntegrationTest
{
    [Fact]
    public void ReferToGlobalVariable()
    {
        const string source = """
                              var x = 1;
                              
                              function f() {
                                print x;
                              }
                              
                              f();
                              """;
        
        RunTest(source, "1");
    }

    [Fact]
    public void LocalClosure()
    {
        
        const string source = """
                              {
                                var x = 1;
                                  
                                function f() {
                                  print x;
                                }
                                  
                                f();
                              }
                              """;
        
        RunTest(source, "1");
    }

    [Fact]
    public void Counter()
    {
        const string source = """
                              function makeCounter() {
                                var count = 0;
                                
                                function increment() {
                                  count = count + 1;
                                  print count;
                                }
                                
                                return increment;
                              }
                              
                              var counter = makeCounter();
                              
                              counter();    // 1
                              counter();    // 2
                              counter();    // 3
                              """;

        var expected = $"1{Environment.NewLine}2{Environment.NewLine}3";
        
        RunTest(source, expected);
    }
    
    [Fact]
    public void StaticResolution()
    {
        const string source = """
                              var x = "global";
                              
                              {
                                function f() {
                                  print x;  // Should always refer to global
                                }
                                
                                f(); // global
                                var x = "inner";
                                f(); // global - not inner
                              }
                              """;
        
        var expected = $"\"global\"{Environment.NewLine}\"global\"";
        
        RunTest(source, expected);
    }
    
    [Fact]
    public void AssignToShadowed()
    {
        const string source = """
                              var x = "global";
                              
                              {
                                function f() {
                                  x = "assigned";
                                }
                                
                                var x = "inner";
                                f();
                                print x;  // "inner"
                              }
                              
                              print x;    // "assigned"
                              """;

        var expected = $"\"inner\"{Environment.NewLine}\"assigned\"";
        
        RunTest(source, expected);
    }
}