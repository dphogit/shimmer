using System.Text;
using Shimmer.Parsing.Expressions;
using Shimmer.Parsing.Statements;
using Shimmer.Representation;
using Shimmer.Scanning;

namespace Shimmer.Parsing;

public class Parser
{
    public bool HadError { get; private set; } = false;
    
    private Token _current = null!;
    private Token _prev = null!;

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

    public Parser(string source, TextWriter errorWriter) : this(new Scanner(source), errorWriter)
    {
    }

    public IList<Stmt> Parse()
    {
        List<Stmt> program = [];
        
        while (!Match(TokenType.Eof))
        {
            try
            {
                program.Add(Declaration());
            }
            catch (ParseException)
            {
                Synchronize();
            }
        }

        return program;
    }

    private Stmt Declaration()
    {
        if (Match(TokenType.Var))
            return VarDecl();
        
        return Statement();
    }

    private VarStmt VarDecl()
    {
        var name = Consume(TokenType.Identifier, "Expected variable name.");
        var initializer = Match(TokenType.Equal) ? Expression() : null;
        Consume(TokenType.SemiColon, "Expect ';' after variable declaration.");
        return new VarStmt(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.Print))
            return PrintStmt();

        if (Match(TokenType.LeftBrace))
            return Block();

        return ExprStmt();
    }

    private PrintStmt PrintStmt()
    {
        var expr = Expression();
        Consume(TokenType.SemiColon, "Expect ';' after print expression.");
        return new PrintStmt(expr);
    }

    private BlockStmt Block()
    {
        List<Stmt> statements = [];

        while (!Check(TokenType.RightBrace) && !Check(TokenType.Eof))
            statements.Add(Declaration());

        Consume(TokenType.RightBrace, "Expect '}' at end of block.");
        return new BlockStmt(statements);
    }

    private ExprStmt ExprStmt()
    {
        var expr = Expression();
        Consume(TokenType.SemiColon, "Expect ';' after previous expression.");
        return new ExprStmt(expr);
    }

    private Expr Expression() => Comma();
    
    private Expr Comma() => LeftAssociativeBinaryOperator(Assignment, TokenType.Comma);

    private Expr Assignment()
    {
        var expr = Conditional();   // If parsing an actual assignment, this is expected be the target (VarExpr).

        if (!Check(TokenType.Equal))
            return expr;
        
        var assignmentTarget = _prev;
            
        Advance();  // Consume '='

        var value = Assignment();

        if (expr is not VarExpr varExpr)
            throw Error(assignmentTarget, "Invalid assignment target.");

        return new AssignExpr(varExpr.Name, value);
    }

    private Expr Conditional()
    {
        var expr = LogicOr(); // If parsing an actual conditional expression, then expr is the condition.

        if (!Match(TokenType.Question))
            return expr;
        
        var thenExpr = Expression();
        Consume(TokenType.Colon, "Expect ':' after truthy branch of conditional.");
        var elseExpr = Conditional();
        
        return new ConditionalExpr(expr, thenExpr, elseExpr);
    }

    private Expr LogicOr() => LeftAssociativeBinaryOperator(LogicAnd, TokenType.Or);

    private Expr LogicAnd() => LeftAssociativeBinaryOperator(Equality, TokenType.And);

    private Expr Equality() => LeftAssociativeBinaryOperator(Comparison, TokenType.EqualEqual, TokenType.BangEqual);

    private Expr Comparison() => LeftAssociativeBinaryOperator(Term, TokenType.Less, TokenType.LessEqual,
        TokenType.Greater, TokenType.GreaterEqual);

    private Expr Term() => LeftAssociativeBinaryOperator(Factor, TokenType.Plus, TokenType.Minus, TokenType.Percent);

    private Expr Factor() => LeftAssociativeBinaryOperator(Unary, TokenType.Star, TokenType.Slash);

    private Expr Unary() => Match(TokenType.Minus, TokenType.Bang) ? new UnaryExpr(_prev, Unary()) : Primary();

    private Expr Primary()
    {
        if (Match(TokenType.Number))
            return new LiteralExpr(ShimmerValue.Number(double.Parse(_prev.Lexeme)));

        if (Match(TokenType.String))
            return new LiteralExpr(ShimmerValue.String(_prev.Lexeme[1..^1]));

        if (Match(TokenType.LeftParen))
            return Grouping();

        if (Match(TokenType.False))
            return LiteralExpr.False;

        if (Match(TokenType.True))
            return LiteralExpr.True;

        if (Match(TokenType.Nil))
            return LiteralExpr.Nil;

        if (Match(TokenType.Identifier))
            return new VarExpr(_prev);

        throw Error(_current, "Expected expression.");
    }

    private GroupExpr Grouping()
    {
        var innerExpr = Expression();
        Consume(TokenType.RightParen, "Expected ')' after previous expression.");
        return new GroupExpr(innerExpr);
    }

    private Expr LeftAssociativeBinaryOperator(Func<Expr> rule, params TokenType[] types)
    {
        var expr = rule();

        while (Match(types))
        {
            var op = _prev;
            var right = rule();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private void Advance()
    {
        _prev = _current;
        _current = _scanner.NextToken();

        if (_current.Type == TokenType.Error)
        {
            Error(_current, _current.Lexeme);
            Synchronize();
        }
    }

    private bool Match(params TokenType[] types)
    {
        if (!types.Contains(_current.Type))
            return false;

        Advance();
        return true;
    }

    private bool Check(params TokenType[] types) => types.Contains(_current.Type);

    private Token Consume(TokenType type, string message)
    {
        var token = _current;
        
        if (token.Type != type)
            throw Error(token, message);
        
        Advance();
        return token;
    }

    private ParseException Error(Token token, string message)
    {
        HadError = true;
        
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

    // Skip tokens until a statement boundary is reached.
    private void Synchronize()
    {
        while (_current.Type != TokenType.Eof)
        {
            // End of statement
            if (_prev?.Type is TokenType.SemiColon or TokenType.RightBrace)
                return;

            // Currently at the start of a new statement
            if (Keywords.StatementStarters.Contains(_current.Type))
                return;

            _prev = _current;
            _current = _scanner.NextToken();
        }
    }
}