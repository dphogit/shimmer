using Shimmer.Scanning;

namespace Shimmer.UnitTests.Scanning;

public class ScannerTests
{
    private readonly TokenFactory _tokenFactory = new();

    [Theory]

    // Single character
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Star)]
    [InlineData("/", TokenType.Slash)]
    [InlineData("=", TokenType.Equal)]
    [InlineData("%", TokenType.Percent)]
    [InlineData("!", TokenType.Bang)]
    [InlineData("<", TokenType.Less)]
    [InlineData(">", TokenType.Greater)]
    [InlineData("(", TokenType.LeftParen)]
    [InlineData(")", TokenType.RightParen)]
    [InlineData(",", TokenType.Comma)]
    [InlineData(":", TokenType.Colon)]
    [InlineData(";", TokenType.SemiColon)]
    [InlineData("?", TokenType.Question)]
    [InlineData("{", TokenType.LeftBrace)]
    [InlineData("}", TokenType.RightBrace)]

    // Two characters
    [InlineData("<=", TokenType.LessEqual)]
    [InlineData("==", TokenType.EqualEqual)]
    [InlineData(">=", TokenType.GreaterEqual)]
    [InlineData("!=", TokenType.BangEqual)]
    [InlineData("123", TokenType.Number)]
    [InlineData("myVariableName", TokenType.Identifier)]
    [InlineData("\"stringLiteral\"", TokenType.String)]

    // Keywords
    [InlineData("print", TokenType.Print)]
    [InlineData("&&", TokenType.And)]
    [InlineData("||", TokenType.Or)]
    [InlineData("false", TokenType.False)]
    [InlineData("if", TokenType.If)]
    [InlineData("else", TokenType.Else)]
    [InlineData("while", TokenType.While)]
    [InlineData("for", TokenType.For)]
    [InlineData("break", TokenType.Break)]
    [InlineData("continue", TokenType.Continue)]
    [InlineData("do", TokenType.Do)]
    [InlineData("switch", TokenType.Switch)]
    [InlineData("case", TokenType.Case)]
    [InlineData("default", TokenType.Default)]
    [InlineData("true", TokenType.True)]
    [InlineData("nil", TokenType.Nil)]
    [InlineData("var", TokenType.Var)]
    [InlineData("", TokenType.Eof)]
    public void NextToken_ReturnsExpectedToken(string source, TokenType type)
    {
        // Arrange
        var scanner = new Scanner(source);
        var expectedToken = _tokenFactory.Create(source, type);

        // Act
        var token = scanner.NextToken();

        // Assert
        Assert.Equal(expectedToken, token);
    }

    [Fact]
    public void NextToken_MultipleTokens_ReturnsExpectedTokens()
    {
        // Arrange
        var scanner = new Scanner("1 + 2");

        var expectedToken1 = _tokenFactory.Number("1");

        _tokenFactory.SetColumn(3);
        var expectedToken2 = _tokenFactory.Plus();

        _tokenFactory.SetColumn(5);
        var expectedToken3 = _tokenFactory.Number("2");

        // Act
        var token1 = scanner.NextToken();
        var token2 = scanner.NextToken();
        var token3 = scanner.NextToken();

        // Assert
        Assert.Equal(expectedToken1, token1);
        Assert.Equal(expectedToken2, token2);
        Assert.Equal(expectedToken3, token3);
    }

    [Fact]
    public void NextToken_Whitespace_ReturnsTokenAtCorrectLocation()
    {
        // Arrange
        var scanner = new Scanner("\n 123");

        _tokenFactory.SetLine(2);
        _tokenFactory.SetColumn(2);
        var expectedToken = _tokenFactory.Number("123");

        // Act
        var token = scanner.NextToken();

        // Assert
        Assert.Equal(expectedToken, token);
    }

    [Theory]
    [InlineData("// inline comment.\nmyVar", 2, 1, "myVar", TestDisplayName = "Inline Comment")]
    [InlineData("/**/ myVar", 1, 6, "myVar", TestDisplayName = "Block Comment")]
    [InlineData("/*\n*/\nmyVar", 3, 1, "myVar", TestDisplayName = "Multiple Line Block Comment")]
    [InlineData("/*\n *\n */\nmyVar", 4, 1, "myVar", TestDisplayName = "Doc Style Comment")]
    public void NextToken_Comment_SkipsAndReturnsTokenAtCorrectLocation(
        string source, int line, int column, string identifier)
    {
        // Arrange
        var scanner = new Scanner(source);

        _tokenFactory.SetLine(line);
        _tokenFactory.SetColumn(column);
        var expectedToken = _tokenFactory.Identifier(identifier);

        // Act
        var token = scanner.NextToken();

        // Assert
        Assert.Equal(expectedToken, token);
    }

    [Fact]
    public void NextToken_BlockCommentUnterminated_ReturnsErrorToken()
    {
        // Arrange
        var scanner = new Scanner("/* Unterminated comment ");
        var errorToken = _tokenFactory.Error("Unterminated block comment.");

        // Act
        var token = scanner.NextToken();

        // Assert
        Assert.Equal(errorToken, token);
    }

    [Fact]
    public void NextToken_UnexpectedCharacter_ReturnsErrorToken()
    {
        // Arrange
        var scanner = new Scanner("$");
        var errorToken = _tokenFactory.Error("Unexpected character '$'.");

        // Act
        var token = scanner.NextToken();

        // Assert
        Assert.Equal(errorToken, token);
    }

    [Fact]
    public void NextToken_StringUnterminated_ReturnsErrorToken()
    {
        // Arrange
        var scanner = new Scanner("\"unterminated");
        var errorToken = _tokenFactory.Error("Unterminated string.");

        // Act
        var token = scanner.NextToken();

        // Assert
        Assert.Equal(errorToken, token);
    }

    [Theory]
    [InlineData('|')]
    [InlineData('&')]
    public void NextToken_UnexpectedSingleChar_ReturnsErrorToken(char c)
    {
        // Arrange
        var scanner = new Scanner($"{c}");
        var errorToken = _tokenFactory.Error($"'{c}' not supported. Did you mean '{c}{c}'?");

        // Act
        var token = scanner.NextToken();

        // Assert
        Assert.Equal(errorToken, token);
    }
}