using Shimmer.Parsing;
using Shimmer.Parsing.Expressions;
using Shimmer.Parsing.Statements;
using Shimmer.Resolving;
using Shimmer.UnitTests.Helpers;

namespace Shimmer.UnitTests.Resolving;

public class ResolverTests
{
    private static IList<Stmt> GetAst(string source) => new Parser(source).Parse();

    [Fact]
    public void Resolve_Local_ReturnsZeroDistance()
    {
        // Arrange
        const string source = """
                              var a = "outer";   // global, not stored in local scopes stack
                              {
                                var b = "inner"; // stacks[0][b] = true
                                
                                a;               // references outer scope
                                b;               // distances[b] = 0
                              }
                              """;

        var ast = GetAst(source);
        var blockStmt = (BlockStmt)ast[1];
        var b = (ExprStmt)blockStmt.Statements[2];

        var resolver = new Resolver();

        // Act
        var resolutionMap = resolver.Resolve(ast);

        // Assert
        Assert.Single(resolutionMap);
        Assert.Equal(0, resolutionMap[b.Expr]);
    }

    [Fact]
    public void Resolve_Shadow_ReturnsZeroDistance()
    {
        // Arrange
        const string source = """
                              var a = "outer";   // global, not stored in local scopes stack
                              {
                                var a = "inner"; // stacks[0][a] = true
                                a;               // distances[a] = 0
                              }
                              """;

        var ast = GetAst(source);
        var blockStmt = (BlockStmt)ast[1];
        var b = (ExprStmt)blockStmt.Statements[1];

        var resolver = new Resolver();

        // Act
        var resolutionMap = resolver.Resolve(ast);

        // Assert
        Assert.Single(resolutionMap);
        Assert.Equal(0, resolutionMap[b.Expr]);
    }

    [Fact]
    public void Resolve_ForLoop_WithVarDeclInitializer()
    {
        // Arrange
        const string source = "for (var i = 0; i < 5; i = i + 1) i;";
        
        var ast = GetAst(source);
        var forStmt = (ForStmt)ast[0];
        
        var condition = (BinaryExpr)forStmt.Condition!;
        
        var increment = (AssignExpr)forStmt.Increment!.Expr;
        var incrementValue = (BinaryExpr)increment.Value;
        
        var exprStmt = (ExprStmt)forStmt.Body;
        
        var resolver = new Resolver();
        
        // Act
        var resolutionMap = resolver.Resolve(ast);
        
        // Assert
        Assert.Equal(4, resolutionMap.Count);
        Assert.Equal(0, resolutionMap[condition.Left]);
        Assert.Equal(0, resolutionMap[increment]);
        Assert.Equal(0, resolutionMap[incrementValue.Left]);
        Assert.Equal(0, resolutionMap[exprStmt.Expr]);
    }
    
    [Fact]
    public void Resolve_ForLoop_WithVarDeclInitializerAndBlockBody()
    {
        // Arrange
        const string source = "for (var i = 0; i < 5; i = i + 1) { i; }";
        
        var ast = GetAst(source);
        var forStmt = (ForStmt)ast[0];
        
        var condition = (BinaryExpr)forStmt.Condition!;
        
        var increment = (AssignExpr)forStmt.Increment!.Expr;
        var incrementValue = (BinaryExpr)increment.Value;
        
        var body = (BlockStmt)forStmt.Body;
        var exprStmt = (ExprStmt)body.Statements[0];
        
        var resolver = new Resolver();
        
        // Act
        var resolutionMap = resolver.Resolve(ast);
        
        // Assert
        Assert.Equal(4, resolutionMap.Count);
        Assert.Equal(0, resolutionMap[condition.Left]);
        Assert.Equal(0, resolutionMap[increment]);
        Assert.Equal(0, resolutionMap[incrementValue.Left]);
        Assert.Equal(1, resolutionMap[exprStmt.Expr]);
        
    }

    [Theory]
    [InlineData("""
                var a = "outer";
                {
                  var a = a; // reference in own initializer not allowed
                }
                """,
                "[Line 3] Error: Can't read local variable 'a' in its own initializer.",
                TestDisplayName = "Self Reference In Initializer")]
    [InlineData("""
                {
                  var a = 1;
                  var a = 2;  // redefinition not allowed
                }
                """,
                "[Line 3] Error: Variable 'a' already defined in this scope.",
                TestDisplayName = "Variable Redefined Same Local Scope")]
    [InlineData("return;",
                "[Line 1] Error: Can't return from top-level code.",
                TestDisplayName = "Return Top Level")]
    [InlineData("break;",
                "[Line 1] Error: Must be inside a loop to break.",
                TestDisplayName = "Break Outside Loop")]
    [InlineData("continue;",
                "[Line 1] Error: Must be inside a loop to continue.",
                TestDisplayName = "Continue Outside Loop")]
    public void Resolve_GivesExpectedErrors(string source, string expected)
    {
        // Arrange
        var ast = GetAst(source);

        var error = new StringWriter();
        var resolver = new Resolver(error);

        // Act
        resolver.Resolve(ast);

        // Assert
        Assert.True(resolver.HadError);
        error.AssertOutput(expected);
    }
}