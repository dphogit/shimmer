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

    private Expr Expression() => LogicOr();

    private Expr LogicOr() => LeftAssociativeBinaryOperator(LogicAnd, TokenType.Or);
    
    private  Expr LogicAnd() => LeftAssociativeBinaryOperator(Equality, TokenType.And);

    private Expr Equality() => LeftAssociativeBinaryOperator(Comparison, TokenType.EqualEqual, TokenType.BangEqual);

    private Expr Comparison() => LeftAssociativeBinaryOperator(Term, TokenType.Less, TokenType.LessEqual,
        TokenType.Greater, TokenType.GreaterEqual);

    private Expr Term() => LeftAssociativeBinaryOperator(Factor, TokenType.Plus, TokenType.Minus);

    private Expr Factor() => LeftAssociativeBinaryOperator(Unary, TokenType.Star, TokenType.Slash);

    private Expr Unary() => Match(TokenType.Minus, TokenType.Bang) ? new UnaryExpr(_prev, Unary()) : Primary();

    private Expr Primary()
    {
        if (Match(TokenType.Number))
            return Number();

        if (Match(TokenType.LeftParen))
            return Grouping();

        if (Match(TokenType.False))
            return LiteralExpr.False;

        if (Match(TokenType.True))
            return LiteralExpr.True;

        if (Match(TokenType.Nil))
            return LiteralExpr.Nil;

        throw Error(_current, "Expected expression.");
    }

    private LiteralExpr Number() => new(ShimmerValue.Number(double.Parse(_prev.Lexeme)));

    private GroupExpr Grouping()
    {
        var innerExpr = Expression();
        Consume(TokenType.RightParen, "Expected ')' after expression.");
        return new GroupExpr(innerExpr);
    }

    private Expr LeftAssociativeBinaryOperator(Func<Expr> rule, params TokenType[] types)
    {
        var expr = rule();

        while (Match(types))
        {
            var @operator = _prev;
            var right = rule();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

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

    private void Consume(TokenType type, string message)
    {
        if (_current.Type == type)
        {
            Advance();
            return;
        }

        throw Error(_current, message);
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