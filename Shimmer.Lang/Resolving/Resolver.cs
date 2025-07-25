using Shimmer.Parsing.Expressions;
using Shimmer.Parsing.Statements;
using Shimmer.Scanning;

namespace Shimmer.Resolving;

/// <summary>
/// The resolver adds a separate pass between parsing and execution, which resolves the variables in the AST.
/// For each variable, it calculates the fixed/static distance between the variable's usage and its referenced value
/// with respect to the lexical scope.
/// </summary>
/// <param name="errorWriter">Writes errors during resolving pass here.</param>
public class Resolver(TextWriter errorWriter)
{
    // Tracks the type of function the resolver is currently in
    private enum FunctionType
    {
        None,
        Function
    }
    
    private FunctionType _currentFunctionType = FunctionType.None;
    private int _loopDepth = 0;
    
    // Use a stack of scopes to refer to each LOCAL block scope, where the top most element is the innermost scope.
    // A dictionary is used to map the variable name, to whether it has been resolved (ready for usage).
    // If a variable cannot be found in this stack of local scopes, then we assume it is global.
    private readonly Stack<Dictionary<string, bool>> _localScopes = new();

    private readonly Dictionary<Expr, int> _resolutionDict = new();

    public bool HadError { get; private set; }

    public Resolver() : this(Console.Error)
    {
    }

    public IReadOnlyDictionary<Expr, int> Resolve(IList<Stmt> stmts)
    {
        foreach (var stmt in stmts)
            ResolveStmt(stmt);

        return _resolutionDict;
    }

    private void BeginScope()
    {
        _localScopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _localScopes.Pop();
    }

    private void Declare(Token name)
    {
        if (!_localScopes.TryPeek(out var scope))
            return;
        
        if (scope.ContainsKey(name.Lexeme))
            Error(name, $"Variable '{name.Lexeme}' already defined in this scope.");
        
        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (_localScopes.TryPeek(out var scope))
            scope[name.Lexeme] = true;
    }

    // Helper to declare and define at once
    private void Initialize(Token name)
    {
        Declare(name);
        Define(name);
    }

    private void ResolveStmt(Stmt stmt)
    {
        switch (stmt)
        {
            case BlockStmt blockStmt:
                ResolveBlock(blockStmt);
                break;
            case BreakStmt breakStmt:
                CheckLoopDepth(breakStmt.Keyword, "Must be inside a loop to break.");
                break;
            case ContinueStmt continueStmt:
                CheckLoopDepth(continueStmt.Keyword, "Must be inside a loop to continue.");
                break;
            case DoWhileStmt doWhileStmt:
                ResolveDoWhileStmt(doWhileStmt);
                break;
            case ExprStmt exprStmt:
                ResolveExpr(exprStmt.Expr);
                break;
            case ForStmt forStmt:
                ResolveForStmt(forStmt);
                break;
            case FunctionStmt functionStmt:
                ResolveFunctionStmt(functionStmt, FunctionType.Function);
                break;
            case IfStmt ifStmt:
                ResolveIfStmt(ifStmt);
                break;
            case PrintStmt printStmt:
                ResolveExpr(printStmt.Expr);
                break;
            case ReturnStmt returnStmt:
                ResolveReturnStmt(returnStmt);
                break;
            case SwitchStmt switchStmt:
                ResolveSwitchStmt(switchStmt);
                break;
            case VarStmt varStmt:
                ResolveVarStmt(varStmt);
                break;
            case WhileStmt whileStmt:
                ResolveWhileStmt(whileStmt);
                break;
            default:
                throw new ArgumentException($"Cannot resolve statement type: '{stmt.GetType()}'.");
        }
    }

    private void ResolveBlock(BlockStmt blockStmt)
    {
        BeginScope();

        foreach (var stmt in blockStmt.Statements)
            ResolveStmt(stmt);

        EndScope();
    }

    private void ResolveDoWhileStmt(DoWhileStmt doWhileStmt)
    {
        try
        {
            _loopDepth++;
            
            ResolveExpr(doWhileStmt.Condition);
            ResolveStmt(doWhileStmt.Body);
        }
        finally
        {
            _loopDepth--;
        }
    }

    private void ResolveForStmt(ForStmt forStmt)
    {
        // Create a new scope to bind for loops clauses to the same scope as the body.
        BeginScope();
        
        try
        {
            _loopDepth++;
            
            if (forStmt.Initializer is not null) ResolveStmt(forStmt.Initializer);
            ResolveExpr(forStmt.Condition);
            if (forStmt.Increment is not null) ResolveStmt(forStmt.Increment);
            ResolveStmt(forStmt.Body);
        }
        finally 
        {
            _loopDepth--;
        }
        
        EndScope();
    }
    
    private void ResolveFunctionStmt(FunctionStmt functionStmt, FunctionType functionType)
    {
        var enclosingType = _currentFunctionType;
        _currentFunctionType = functionType;
        
        // Function name is bound to the surrounding scope, defining it eagerly allows recursive referencing.
        Initialize(functionStmt.Name);
        
        // Create a new scope for the function's body and bind the parameters to this new scope.
        BeginScope();
        
        foreach (var param in functionStmt.Parameters)
            Initialize(param);
        
        Resolve(functionStmt.Body.Statements);
        
        EndScope();
        
        _currentFunctionType = enclosingType;
    }

    private void ResolveIfStmt(IfStmt ifStmt)
    {
        ResolveExpr(ifStmt.Condition);
        ResolveStmt(ifStmt.ThenBranch);
        if (ifStmt.ElseBranch is not null) ResolveStmt(ifStmt.ElseBranch);
    }

    private void ResolveReturnStmt(ReturnStmt returnStmt)
    {
        if (_currentFunctionType == FunctionType.None)
            Error(returnStmt.Keyword, "Can't return from top-level code.");
        
        ResolveExpr(returnStmt.Expr);
    }

    private void ResolveSwitchStmt(SwitchStmt switchStmt)
    {
        ResolveExpr(switchStmt.Expr);
        
        foreach (var caseClause in switchStmt.Cases)
        {
            ResolveExpr(caseClause.Condition);
            ResolveStmt(caseClause.Stmt);
        }
        
        if (switchStmt.DefaultClause is not null) ResolveStmt(switchStmt.DefaultClause);
    }
    
    private void ResolveVarStmt(VarStmt varStmt)
    {
        // Binding variables are split into two steps, declaring then defining. This allows us to detect and prevent a
        // variable's initializer referring to a variable with the same name as the one being declared.
        Declare(varStmt.Name);
        ResolveExpr(varStmt.Initializer);
        Define(varStmt.Name);
    }

    private void ResolveWhileStmt(WhileStmt whileStmt)
    {
        try
        {
            _loopDepth++;
            
            ResolveExpr(whileStmt.Condition);
            ResolveStmt(whileStmt.Body);
        }
        finally
        {
            _loopDepth--;
        }
    }

    private void ResolveExpr(Expr expr)
    {
        switch (expr)
        {
            case AssignExpr assignExpr:
                ResolveAssignExpr(assignExpr);
                break;
            case BinaryExpr binaryExpr:
                ResolveBinaryExpr(binaryExpr);
                break;
            case CallExpr callExpr:
                ResolveCallExpr(callExpr);
                break;
            case ConditionalExpr conditionalExpr:
                ResolveConditionalExpr(conditionalExpr);
                break;
            case GroupExpr groupExpr:
                ResolveExpr(groupExpr.Expr);
                break;
            case LiteralExpr:
                break;
            case UnaryExpr unaryExpr:
                ResolveExpr(unaryExpr.Expr);
                break;
            case VarExpr varExpr:
                ResolveVarExpr(varExpr);
                break;
            default:
                throw new ArgumentException($"Cannot resolve expression type: '{expr.GetType()}'.");
        }
    }

    private void ResolveAssignExpr(AssignExpr assignExpr)
    {
        ResolveExpr(assignExpr.Value);
        ResolveLocal(assignExpr, assignExpr.Name);
    }

    private void ResolveBinaryExpr(BinaryExpr binaryExpr)
    {
        ResolveExpr(binaryExpr.Left);
        ResolveExpr(binaryExpr.Right);
    }

    private void ResolveCallExpr(CallExpr callExpr)
    {
        ResolveExpr(callExpr.Callee);
        foreach (var arg in callExpr.Arguments) ResolveExpr(arg);
    }

    private void ResolveConditionalExpr(ConditionalExpr conditionalExpr)
    {
        ResolveExpr(conditionalExpr.Condition);
        ResolveExpr(conditionalExpr.ThenExpr);
        ResolveExpr(conditionalExpr.ElseExpr);
    }

    private void ResolveVarExpr(VarExpr varExpr)
    {
        var name = varExpr.Name;

        if (_localScopes.TryPeek(out var scope) && scope.TryGetValue(name.Lexeme, out var initialized) && !initialized)
        {
            Error(name, $"Can't read local variable '{name.Lexeme}' in its own initializer.");
        }

        ResolveLocal(varExpr, name);
    }

    // Walk up from innermost scope outwards and find the number of scopes (distance) between the current scope of the
    // variable usage and where it was found. For example, resolved current local scope gives 0, next one up 1, etc.
    private void ResolveLocal(Expr expr, Token name)
    {
        var distance = 0;

        foreach (var scope in _localScopes)
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                _resolutionDict[expr] = distance;
                return;
            }

            distance++;
        }
    }

    private void CheckLoopDepth(Token keyword, string message)
    {
        if (_loopDepth == 0)
            Error(keyword, message);
    }

    private void Error(Token name, string message)
    {
        HadError = true;
        errorWriter.WriteLine($"[Line {name.Line}] Error: {message}");
    }
}