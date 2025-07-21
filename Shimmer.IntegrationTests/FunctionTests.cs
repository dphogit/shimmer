namespace Shimmer.IntegrationTests;

public class FunctionTests : BaseIntegrationTest
{
    [Fact]
    public void Print_NativeFunction() => RunTest("print clock;", "<native clock>");

    [Fact]
    public void Print_UserDefinedFunction()
    {
        const string source = """
                              function emptyFunc() {}
                              
                              print emptyFunc;
                              """;
        
        RunTest(source, "<fn emptyFunc>");
    }

    [Fact]
    public void ReturnStatement()
    {
        const string source = """
                              // Purposely wrote with if check rather than returning n % 2 == 0
                              // to make sure the return short-circuits the function execution.
                              function isEven(n) {
                                if (n % 2 == 0) return true;
                                return false;
                              }
                              
                              print isEven(1);
                              print isEven(2);
                              """;
        
        RunTest(source, $"false{Environment.NewLine}true");
    }

    [Fact]
    public void Recursion()
    {
        const string source = """
                              function fib(n) {
                                if (n < 2) return n;
                                return fib(n - 1) + fib(n - 2);
                              }
                              
                              print fib(8); // 0, 1, 1, 2, 3, 5, 8, 13, 21
                              """;
        
        RunTest(source, "21");
    }

    [Fact]
    public void NoReturnStatement_ImplicitlyReturnsNil()
    {
        const string source = """
                              function f() {}
                              print f();
                              """;
        RunTest(source, "nil");
    }

    [Fact]
    public void NoReturnExpression_ImplicitlyReturnsNil()
    {
        const string source = """
                              function f() { return; }
                              print f();
                              """;
        RunTest(source, "nil");
    }
    
    [Fact]
    public void BodyIsNotBlock_SyntaxError()
    {
        const string source = "function f() 1;";
        const string expected = "[Line 1, Col 14] Error at '1': Expect '{' before function body.";
        RunErrorTest(source, expected);
    }

    [Fact]
    public void MissingCommaInParameters_SyntaxError()
    {
        const string source = "function printSum(a b) { print a + b; }";
        const string expected = "[Line 1, Col 21] Error at 'b': Expect ')' after parameters.";
        RunErrorTest(source, expected);
    }

    [Fact]
    public void ExtraArguments_RuntimeError()
    {
        const string source = """
                              function printSum(a, b) {
                                print a + b;
                              }
                              
                              printSum(1, 2, 3);
                              """;

        const string expected = "[Line 5] Runtime error: Expected 2 arguments but got 3.";
        RunErrorTest(source, expected);
    }

    [Fact]
    public void MissingArguments_RuntimeError()
    {
        const string source = """
                              function printSum(a, b) {
                                print a + b;
                              }
                              
                              printSum(1);
                              """;

        const string expected = "[Line 5] Runtime error: Expected 2 arguments but got 1.";
        RunErrorTest(source, expected);
    }
}