using Shimmer.Scanning;

namespace Shimmer.UnitTests.Scanning;

public class ScannerTests
{
    private readonly TokenFactory _tokenFactory = new();
    
    [Theory]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Star)]
    [InlineData("/", TokenType.Slash)]
    [InlineData("=", TokenType.Equal)]
    [InlineData("!", TokenType.Bang)]
    [InlineData("(", TokenType.LeftParen)]
    [InlineData(")", TokenType.RightParen)]
    [InlineData("<", TokenType.Less)]
    [InlineData("<=", TokenType.LessEqual)]
    [InlineData(">", TokenType.Greater)]
    [InlineData(">=", TokenType.GreaterEqual)]
    [InlineData("==", TokenType.EqualEqual)]
    [InlineData("!=", TokenType.BangEqual)]
    [InlineData("&&", TokenType.And)]
    [InlineData("||", TokenType.Or)]
    [InlineData("123", TokenType.Number)]
    [InlineData("false", TokenType.False)]
    [InlineData("true", TokenType.True)]
    [InlineData("nil", TokenType.Nil)]
    [InlineData("myVariableName", TokenType.Identifier)]
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