using Shimmer.Parsing;
using Shimmer.Parsing.Statements;
using Shimmer.Scanning;
using Shimmer.UnitTests.Helpers;

namespace Shimmer.UnitTests.Parsing;

public class ParserTests
{
    private static void RunParseTest<TStmt>(string source, string expected) where TStmt : Stmt
    {
        // Arrange
        var parser = new Parser(source);
        
        // Act
        var stmts = parser.Parse();
        
        // Assert
        Assert.False(parser.HadError);
        Assert.Single(stmts);
        var stmt = Assert.IsType<TStmt>(stmts[0]);
        Assert.Equal(expected, stmt.ToString());
    }

    [Theory]
    [InlineData("1 + 2;", "(1 + 2)", TestDisplayName = "Addition")]
    [InlineData("1 - 2;", "(1 - 2)", TestDisplayName = "Subtraction")]
    [InlineData("1 * 2;", "(1 * 2)", TestDisplayName = "Multiplication")]
    [InlineData("1 / 2;", "(1 / 2)", TestDisplayName = "Division")]
    [InlineData("3 % 1;", "(3 % 1)", TestDisplayName = "Remainder")]
    [InlineData("1 < 2;", "(1 < 2)", TestDisplayName = "Less Than")]
    [InlineData("1 <= 2;", "(1 <= 2)", TestDisplayName = "Less Than Equal")]
    [InlineData("1 > 2;", "(1 > 2)", TestDisplayName = "Less Than")]
    [InlineData("1 >= 2;", "(1 >= 2)", TestDisplayName = "Less Than Equal")]
    [InlineData("1 == 2;", "(1 == 2)", TestDisplayName = "Equality")]
    [InlineData("1 != 2;", "(1 != 2)", TestDisplayName = "Inequality")]
    [InlineData("1 + 2 - 3;", "((1 + 2) - 3)", TestDisplayName = "Left Associativity")]
    [InlineData("6 / 3 + 1;", "((6 / 3) + 1)", TestDisplayName = "Operator Precedence")]
    [InlineData("true && true;", "(true && true)", TestDisplayName = "Logical And")]
    [InlineData("true || true;", "(true || true)", TestDisplayName = "Logical Or")]
    [InlineData("1, 2;", "(1 , 2)", TestDisplayName = "Comma")]
    [InlineData("1;", "1", TestDisplayName = "Number")]
    [InlineData("true;", "true", TestDisplayName = "True")]
    [InlineData("false;", "false", TestDisplayName = "False")]
    [InlineData("nil;", "nil", TestDisplayName = "Nil")]
    [InlineData("\"string\";", "\"string\"", TestDisplayName = "String")]
    [InlineData("\"\";", "\"\"", TestDisplayName = "Empty String")]
    [InlineData("-1;", "(- 1)", TestDisplayName = "Unary Minus")]
    [InlineData("!false;", "(! false)", TestDisplayName = "Logical Not")]
    [InlineData("(1 + 2);", "(1 + 2)", TestDisplayName = "Grouping")]
    [InlineData("true ? 1 : 2;", "(true ? 1 : 2)", TestDisplayName = "Conditional")]
    [InlineData("false ? 1 : true ? 2 : 3;", "(false ? 1 : (true ? 2 : 3))", TestDisplayName = "Nested Conditional")]
    [InlineData("x;", "x", TestDisplayName = "Variable")]
    [InlineData("x = 2;", "(x = 2)", TestDisplayName = "Assignment")]
    public void Parse_ExpressionStatements_ReturnsExprStmt(string source, string expected) =>
        RunParseTest<ExprStmt>(source, expected);
    
    [Theory]
    [InlineData("print 1;", "(print 1)")]
    [InlineData("print 1 + 2;", "(print (1 + 2))")]
    [InlineData("print \"foo\";", "(print \"foo\")")]
    public void Parse_PrintStatement_ReturnsPrintStmt(string source, string expected) =>
        RunParseTest<PrintStmt>(source, expected);

    [Theory]
    [InlineData("var x;", "(var x = nil)", TestDisplayName = "No Initializer")]
    [InlineData("var x = 3;", "(var x = 3)", TestDisplayName = "Initializer")]
    [InlineData("var x = 9 * 8;", "(var x = (9 * 8))", TestDisplayName = "Expression Initializer")]
    public void Parse_VariableDeclaration_ReturnsVarStmt(string source, string expected) =>
        RunParseTest<VarStmt>(source, expected);

    [Fact]
    public void Parse_BlockStatement_ReturnsBlockStmt()
    {
        const string source = "{ var x = 1 + 2; }";
        const string expected = "{ (var x = (1 + 2)) }";
        RunParseTest<BlockStmt>(source, expected);
    }

    [Fact]
    public void Parse_IfStatement_ReturnsIfStmt()
    {
        const string source = "if (true) print 1;";
        const string expected = "(if (true) (then (print 1)) )";
        RunParseTest<IfStmt>(source, expected);
    }

    [Fact]
    public void Parse_IfElseStatement_ReturnsIfStmt()
    {
        const string source = "if (true) print 1; else print 2;";
        const string expected = "(if (true) (then (print 1)) (else (print 2)) )";
        RunParseTest<IfStmt>(source, expected);
    }

    [Fact]
    public void Parse_WhileLoop_ReturnsWhileStmt()
    {
        const string source = "while (true) print 1;";
        const string expected = "(while (true) (print 1) )";
        RunParseTest<WhileStmt>(source, expected);
    }

    [Fact]
    public void Parse_ForLoop_ReturnsForStmt()
    {
        const string source = "for (var i = 0; i < 10; i = i + 1) print i;";
        const string expected = "(for (var i = 0) (i < 10) (i = (i + 1)) (print i))";
        RunParseTest<ForStmt>(source, expected);
    }
    
    [Fact]
    public void Parse_ForLoopNoInitializer_ReturnsForStmt()
    {
        const string source = "for (; i < 10; i = i + 1) print i;";
        const string expected = "(for () (i < 10) (i = (i + 1)) (print i))";
        RunParseTest<ForStmt>(source, expected);
    }

    [Fact]
    public void Parse_ForLoopNoIncrement_ReturnsForStmt()
    {
        const string source = "for (var i = 0; i < 10;) print i;";
        const string expected = "(for (var i = 0) (i < 10) () (print i))";
        RunParseTest<ForStmt>(source, expected);
    }

    [Fact]
    public void Parse_ForLoopNoCondition_ReturnsForStmt()
    {
        const string source = "for (var i = 0; ; i = i + 1) print i;";
        const string expected = "(for (var i = 0) true (i = (i + 1)) (print i))";
        RunParseTest<ForStmt>(source, expected);
    }

    [Fact]
    public void Parse_DoWhileLoop_ReturnsDoWhileStmt()
    {
        const string source = "do { print true; } while (true);";
        const string expected = "(do { (print true) } while (true) )";
        RunParseTest<DoWhileStmt>(source, expected);
    }

    [Fact]
    public void Parse_SwitchStatement_ReturnsSwitchStmt()
    {
        const string source = """
                              switch (1) {
                                case 1: print 1;
                                case 2: print 2;
                                default: print 3;
                              }
                              """;
        const string expected = "(switch (1) (case 1 : (print 1)) (case 2 : (print 2)) (default : (print 3)) )";
        RunParseTest<SwitchStmt>(source, expected);
    }

    [Fact]
    public void Parse_FunctionCall_ReturnsCallExprStmt()
    {
        const string source = "avg(1, 1 + 2);";
        const string expected = "(call avg 1 (1 + 2) )";
        RunParseTest<ExprStmt>(source, expected);
    }

    [Fact]
    public void Parse_FunctionCallNoArgs_ReturnsCallExprStmt()
    {
        const string source = "avg();";
        const string expected = "(call avg)";
        RunParseTest<ExprStmt>(source, expected);
    }

    [Fact]
    public void Parse_FunctionDeclaration_ReturnsFunctionStmt()
    {
        const string source = "function add(a, b) { print a + b; }";
        const string expected = "(fn add(a, b) { (print (a + b)) } )";
        RunParseTest<FunctionStmt>(source, expected);
    }

    [Fact]
    public void Parse_FunctionDeclarationNoArgs_ReturnsFunctionStmt()
    {
        const string source = "function printOne() { print 1; }";
        const string expected = "(fn printOne() { (print 1) } )";
        RunParseTest<FunctionStmt>(source, expected);
    }

    [Fact]
    public void Parse_ReturnStatement_ReturnsReturnStmt() => 
        RunParseTest<ReturnStmt>("return 1;", "(return 1)");
    
    [Theory]
    [InlineData("1 + *", "[Line 1, Col 5] Error at '*': Expected expression.", TestDisplayName = "Invalid Expression")]
    [InlineData("(1 + 2", "[Line 1, Col 6] Error at end: Expected ')' after previous expression.",
        TestDisplayName = "No Closing Parenthesis")]
    [InlineData("1 + 2)", "[Line 1, Col 6] Error at ')': Expect ';' after previous expression.",
        TestDisplayName = "No Opening Parenthesis")]
    [InlineData("$", "[Line 1, Col 1] Error: Unexpected character '$'.", TestDisplayName = "Unexpected Character")]
    [InlineData("{ 1;", "[Line 1, Col 4] Error at end: Expect '}' at end of block.",
        TestDisplayName = "No Closing Brace")]
    public void Parse_InvalidSyntax_ReportsError(string source, string expected)
    {
        // Arrange
        var scanner = new Scanner(source);
        var errorWriter = new StringWriter();

        var parser = new Parser(scanner, errorWriter);

        // Act
        var stmts = parser.Parse();

        // Assert
        Assert.True(parser.HadError);
        Assert.Empty(stmts);
        errorWriter.AssertOutput(expected);
    }

    [Theory]
    [InlineData("1 + *; 2 + 3;", "[Line 1, Col 5] Error at '*': Expected expression.")]
    [InlineData("/ + 2; 2 + 3;", "[Line 1, Col 1] Error at '/': Expected expression.")]
    [InlineData("1 + 2 print 2 + 3;", "[Line 1, Col 7] Error at 'print': Expect ';' after previous expression.")]
    public void Parse_ErrorInOneStatement_ReportsAndSkipsToNextStatement(string source, string expected)
    {
        // Arrange
        var errorWriter = new StringWriter();
        var parser = new Parser(source, errorWriter);
        
        // Act
        var stmts = parser.Parse();
        
        // Assert
        Assert.True(parser.HadError);
        Assert.Single(stmts);
        errorWriter.AssertOutput(expected);
    }
}