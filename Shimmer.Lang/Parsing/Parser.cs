using System.Text;
using Shimmer.Parsing.Expressions;
using Shimmer.Representation;
using Shimmer.Scanning;

namespace Shimmer.Parsing;

public class Parser 
{
    private Token _current;
    private Token _prev;

    private class ParseException : Exception;

    private readonly IScanner _scanner;
    private readonly TextWriter _errorWriter;

    public Parser(IScanner scanner, TextWriter errorWriter)
    {
        _scanner = scanner;
        _errorWriter = errorWriter;

        Advance();
    }

    public Parser(string source) : this(new Scanner(source), Console.Error)
    {
    }

    public Expr? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseException)
        {
            // TODO: Implement synchronization for error recovery when adding statements.
            return null;
        }
    }

    private Expr Expression() => Term();

    private Expr Term()
    {
        var expr = Primary();

        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = _prev;
            var right = Primary();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(TokenType.Number))
            return Number();

        throw Error(_current, "Expected expression.");
    }

    private LiteralExpr Number() => new(ShimmerValue.Number(double.Parse(_prev.Lexeme)));

    private void Advance()
    {
        _prev = _current;

        while (true)
        {
            _current = _scanner.NextToken();
            
            if (_current.Type == TokenType.Error)
            {
                Error(_current, _current.Lexeme);
                continue;
            }

            return;
        }
    }

    private bool Match(params TokenType[] types)
    {
        if (!types.Contains(_current.Type))
            return false;

        Advance();
        return true;
    }

    private ParseException Error(Token token, string message)
    {
        // TODO: Add Panic Mode Flag to not cascade errors - add during synchronization.
        
        StringBuilder sb = new($"[Line {token.Line}, Col {token.Column}] Error");

        var location = token.Type switch
        {
            TokenType.Error => "",
            TokenType.Eof => " at end",
            _ => $" at '{token.Lexeme}'",
        };

        sb.Append($"{location}: {message}");
        
        _errorWriter.Write(sb.ToString());
        return new ParseException();
    }
}