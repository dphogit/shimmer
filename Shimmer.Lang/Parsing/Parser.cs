using System.Text;
using Shimmer.Parsing.Expressions;
using Shimmer.Parsing.Statements;
using Shimmer.Representation;
using Shimmer.Scanning;

namespace Shimmer.Parsing;

public class Parser
{
    private const int MaxArguments = 255;
    private const int MaxParameters = MaxArguments;
    
    public bool HadError { get; private set; } = false;
    
    private Token _current = null!;
    private Token _prev = null!;

    private class ParseException(string message) : Exception(message);

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

        if (Match(TokenType.Function))
            return FunctionDecl();
        
        return Statement();
    }

    private VarStmt VarDecl()
    {
        var name = Consume(TokenType.Identifier, "Expected variable name.");
        var initializer = Match(TokenType.Equal) ? Expression() : null;
        Consume(TokenType.SemiColon, "Expect ';' after variable declaration.");
        return new VarStmt(name, initializer);
    }

    private FunctionStmt FunctionDecl()
    {
        var name = Consume(TokenType.Identifier, "Expect function name.");
        
        Consume(TokenType.LeftParen, "Expect '(' after function name.");
        var parameters = FunctionParameters();
        Consume(TokenType.RightParen, "Expect ')' after parameters.");
        
        Consume(TokenType.LeftBrace, "Expect '{' before function body.");
        var body = Block();

        return new FunctionStmt(name, parameters, body);
    }

    private List<Token> FunctionParameters()
    {
        if (Check(TokenType.RightParen))
            return [];

        List<Token> parameters = [];

        do
        {
            if (parameters.Count >= MaxParameters)
                Error(_current, $"Exceeded maximum of {MaxParameters} parameters.");
            
            var param = Consume(TokenType.Identifier, "Expect parameter name.");
            parameters.Add(param);
        } while (Match(TokenType.Comma));

        return parameters;
    }

    private Stmt Statement()
    {
        if (Match(TokenType.Print))
            return PrintStmt();

        if (Match(TokenType.LeftBrace))
            return Block();

        if (Match(TokenType.If))
            return IfStmt();

        if (Match(TokenType.Switch))
            return SwitchStmt();

        if (Match(TokenType.While))
            return WhileStmt();

        if (Match(TokenType.For))
            return ForStmt();

        if (Match(TokenType.Do))
            return DoWhileStmt();

        if (Match(TokenType.Break))
            return BreakStmt();

        if (Match(TokenType.Continue))
            return ContinueStmt();

        if (Match(TokenType.Return))
            return ReturnStmt();

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

    private IfStmt IfStmt()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after 'if' condition.");

        var thenBranch = Statement();
        var elseBranch = Match(TokenType.Else) ? Statement() : null;

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    private SwitchStmt SwitchStmt()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'switch'.");
        var expression = Expression();
        Consume(TokenType.RightParen, "Expect ')' after 'switch' expression.");

        Consume(TokenType.LeftBrace, "Expect '{' at start of 'switch' body.");

        List<SwitchStmt.CaseClause> cases = [];
        while (Match(TokenType.Case))
            cases.Add(CaseClause());

        var defaultCase = Match(TokenType.Default) ? DefaultCase() : null;
        
        Consume(TokenType.RightBrace, "Expect '}' at end of 'switch' body.");
        
        return new SwitchStmt(expression, cases, defaultCase);
    }

    private SwitchStmt.CaseClause CaseClause()
    {
        var expression = Expression();
        Consume(TokenType.Colon, "Expect ':' after 'case' expression.");
        var statement = Statement();
        return new SwitchStmt.CaseClause(expression, statement);
    }

    private Stmt DefaultCase()
    {
        Consume(TokenType.Colon, "Expect ':' after 'default'.");
        return Statement();
    }
    
    private WhileStmt WhileStmt()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after 'while' condition.");

        var body = Statement();
        return new WhileStmt(condition, body);
    }

    private ForStmt ForStmt()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");
        
        Stmt? initializer;
        if (Match(TokenType.SemiColon))
            initializer = null;
        else if (Match(TokenType.Var))
            initializer = VarDecl();
        else
            initializer = ExprStmt();
        
        // If no condition is given, we default it to true.
        var condition = Check(TokenType.SemiColon) ? LiteralExpr.True : Expression();
        Consume(TokenType.SemiColon, "Expect ';' after 'for' condition.");
        
        var increment = Check(TokenType.RightParen) ? null : new ExprStmt(Expression());
        Consume(TokenType.RightParen, "Expect ')' after 'for' clauses.");
        
        var body = Statement();
        
        return new ForStmt(initializer, condition, increment, body);
    }

    private DoWhileStmt DoWhileStmt()
    {
        var body = Statement();

        Consume(TokenType.While, "Expect 'while' after 'do' body.");
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after 'while' condition.");
        Consume(TokenType.SemiColon, "Expect ';' after 'do-while' condition.");

        return new DoWhileStmt(body, condition);
    }
    
    private BreakStmt BreakStmt()
    {
        var keyword = _prev;
        Consume(TokenType.SemiColon, "Expect ';' after 'break'.");
        return new BreakStmt(keyword);
    }

    private ContinueStmt ContinueStmt()
    {
        var keyword = _prev;
        Consume(TokenType.SemiColon, "Expect ';' after 'continue'.");
        return new ContinueStmt(keyword);
    }

    private ReturnStmt ReturnStmt()
    {
        var keyword = _prev;
        var expression = Check(TokenType.SemiColon) ? LiteralExpr.Nil : Expression();
        Consume(TokenType.SemiColon, "Expect ';' at end of 'return' statement.");
        return new ReturnStmt(keyword, expression);
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
        var expr = Conditional(); // If parsing an actual assignment, this is expected be the target (VarExpr).

        if (!Check(TokenType.Equal))
            return expr;
        
        var assignmentTarget = _prev;

        Advance(); // Consume '='

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

    private Expr Unary() => Match(TokenType.Minus, TokenType.Bang) ? new UnaryExpr(_prev, Unary()) : Call();

    private Expr Call()
    {
        var expr = Primary(); // If parsing a function call, this is the callee.

        while (Match(TokenType.LeftParen))
        {
            expr = FinishCall(expr);
        }

        return expr;
    }

    private CallExpr FinishCall(Expr callee)
    {
        // Initial check for no arguments to parse
        if (Match(TokenType.RightParen))
            return new CallExpr(callee, _prev, []);
        
        List<Expr> arguments = [];

        do
        {
            if (arguments.Count >= MaxArguments)
                Error(_current, $"Exceeded maximum of {MaxArguments} arguments.");
            
            arguments.Add(Assignment());
        } while (Match(TokenType.Comma));

        var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");
        
        return new CallExpr(callee, paren, arguments);
    }

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
        
        var errorMessage = sb.ToString();

        _errorWriter.WriteLine(errorMessage);
        return new ParseException(errorMessage);
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