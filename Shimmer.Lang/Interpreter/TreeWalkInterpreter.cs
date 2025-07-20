using Shimmer.Parsing.Expressions;
using Shimmer.Parsing.Statements;
using Shimmer.Representation;
using Shimmer.Scanning;

namespace Shimmer.Interpreter;

public class TreeWalkInterpreter : IInterpreter
{
    private class BreakException : Exception;
    private class ContinueException : Exception;
    
    private Environment _environment = new();
    
    private readonly TextWriter _outputWriter;
    private readonly TextWriter _errorWriter;

    /// <summary>
    /// The interpreter walks the given AST in its <see cref="Interpret"/> method,
    /// redirecting output to <paramref name="outputWriter" /> and errors
    /// to <paramref name="errorWriter"/>
    /// </summary>
    /// <param name="outputWriter">Defaults to <see cref="Console.Out"/>.</param>
    /// <param name="errorWriter">Defaults to <see cref="Console.Error"/>.</param>
    public TreeWalkInterpreter(TextWriter? outputWriter = null, TextWriter? errorWriter = null)
    {
        _outputWriter = outputWriter ?? Console.Out;
        _errorWriter = errorWriter ?? Console.Error;
    }

    public void Interpret(IList<Stmt> stmts)
    {
        try
        {
            foreach (var stmt in stmts)
                Execute(stmt);
        }
        catch (RuntimeError e)
        {
            _errorWriter.WriteLine(e.Message);
        }
    }

    private void Execute(Stmt stmt)
    {
        switch (stmt)
        {
            case BlockStmt blockStmt:
                ExecuteBlockStmt(blockStmt, new Environment(_environment));
                break;
            case BreakStmt:
                throw new BreakException();
            case ContinueStmt:
                throw new ContinueException();
            case DoWhileStmt doWhileStmt:
                ExecuteDoWhileStmt(doWhileStmt);
                break;
            case ExprStmt exprStmt:
                Eval(exprStmt.Expr);
                break;
            case IfStmt ifStmt:
                ExecuteIfStmt(ifStmt);
                break;
            case PrintStmt printStmt:
                ExecutePrintStmt(printStmt);
                break;
            case SwitchStmt switchStmt:
                ExecuteSwitchStmt(switchStmt);
                break;
            case VarStmt varStmt:
                ExecuteVarStmt(varStmt);
                break;
            case WhileStmt whileStmt:
                ExecuteWhileStmt(whileStmt);
                break;
            default:
                throw new ArgumentException($"Cannot execute statement type '{stmt.GetType().Name}.");
        }
    }

    private void ExecuteBlockStmt(BlockStmt blockStmt, Environment environment)
    {
        // Save reference to the previous environment so we can restore it once entering this block's scope.
        var previous = _environment;

        try
        {
            _environment = environment;

            foreach (var stmt in blockStmt.Statements)
                Execute(stmt);
        }
        finally
        {
            _environment = previous;
        }
    }

    private void ExecuteDoWhileStmt(DoWhileStmt doWhileStmt)
    {
        try
        {
            do
            {
                try
                {
                    Execute(doWhileStmt.Body);
                }
                catch (ContinueException)
                {
                    // continue statement - go to next iteration
                }
            } while (IsTruthy(Eval(doWhileStmt.Condition)));
        }
        catch (BreakException)
        {
            // break statement - exit loop
        }
    }
    
    private void ExecuteIfStmt(IfStmt ifStmt)
    {
        var condition = Eval(ifStmt.Condition);

        if (IsTruthy(condition))
        {
            Execute(ifStmt.ThenBranch);
        }
        else if (ifStmt.ElseBranch is not null)
        {
            Execute(ifStmt.ElseBranch);
        }
    }

    private void ExecutePrintStmt(PrintStmt printStmt) => _outputWriter.WriteLine(Eval(printStmt.Expr).ToString());

    private void ExecuteSwitchStmt(SwitchStmt switchStmt)
    {
        var expression = Eval(switchStmt.Expr);

        // Execute the first case that matches the expression.
        foreach (var caseClause in switchStmt.Cases)
        {
            if (!Eval(caseClause.Condition).Equals(expression))
                continue;
            
            Execute(caseClause.Stmt);
            return;
        }

        // No cases match the expression, execute the default case if given.
        if (switchStmt.DefaultClause is not null)
            Execute(switchStmt.DefaultClause);
    }
    
    private void ExecuteVarStmt(VarStmt varStmt) => _environment.Define(varStmt.Name, Eval(varStmt.Initializer));

    private void ExecuteWhileStmt(WhileStmt whileStmt)
    {
        try
        {
            while (IsTruthy(Eval(whileStmt.Condition)))
            {
                try
                {
                    Execute(whileStmt.Body);
                }
                catch (ContinueException)
                {
                    // continue statement - increment if the clause is given (for loops)
                    if (whileStmt.Increment is not null)
                        Execute(whileStmt.Increment);
                }
            }
        }
        catch (BreakException)
        {
            // break statement - exit loop
        }
    }

    private ShimmerValue Eval(Expr expr) => expr switch
    {
        AssignExpr assignExpr => EvalAssignExpr(assignExpr),
        BinaryExpr binaryExpr => EvalBinaryExpr(binaryExpr),
        ConditionalExpr conditionalExpr => EvalConditionalExpr(conditionalExpr),
        GroupExpr groupExpr => Eval(groupExpr.Expr),
        LiteralExpr literalExpr => literalExpr.Value,
        UnaryExpr unaryExpr => EvalUnaryExpr(unaryExpr),
        VarExpr varExpr => EvalVarExpr(varExpr),
        _ => throw new ArgumentException($"Cannot evaluate expression type '{expr.GetType().Name}'.")
    };

    private ShimmerValue EvalAssignExpr(AssignExpr assignExpr)
    {
        var value = Eval(assignExpr.Value);
        _environment.Assign(assignExpr.Name, value);
        return value;
    }

    private ShimmerValue EvalBinaryExpr(BinaryExpr binaryExpr)
    {
        var op = binaryExpr.Op;

        // First check for logical operators because we don't want to evaluate
        // both operands as these operations can short-circuit on the first operand.
        switch (op.Type)
        {
            case TokenType.And:
            {
                var leftAnd = Eval(binaryExpr.Left);
                return IsFalsy(leftAnd) ? leftAnd : Eval(binaryExpr.Right);
            }
            case TokenType.Or:
            {
                var leftOr = Eval(binaryExpr.Left);
                return IsTruthy(leftOr) ? leftOr : Eval(binaryExpr.Right);
            }
        }

        ShimmerValue left = Eval(binaryExpr.Left), right = Eval(binaryExpr.Right);

        switch (op.Type)
        {
            case TokenType.Plus:
            {
                if (left.IsString && right.IsString)
                    return ShimmerValue.String(left.AsString + right.AsString);
                        
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber + right.AsNumber);
            }
            case TokenType.Minus:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber - right.AsNumber);
            }
            case TokenType.Star:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber * right.AsNumber);
            }
            case TokenType.Slash:
            {
                CheckOperandsAreNumbers(left, right);

                if (right.AsNumber == 0)
                    throw RuntimeError.Create(op, "Division by 0.");

                return ShimmerValue.Number(left.AsNumber / right.AsNumber);
            }
            case TokenType.Percent:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Number(left.AsNumber % right.AsNumber);
            }
            case TokenType.Less:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber < right.AsNumber);
            }
            case TokenType.LessEqual:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber <= right.AsNumber);
            }
            case TokenType.EqualEqual:
            {
                return ShimmerValue.Bool(left.Equals(right));
            }
            case TokenType.BangEqual:
            {
                return ShimmerValue.Bool(!left.Equals(right));
            }
            case TokenType.Greater:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber > right.AsNumber);
            }
            case TokenType.GreaterEqual:
            {
                CheckOperandsAreNumbers(left, right);
                return ShimmerValue.Bool(left.AsNumber >= right.AsNumber);
            }
            case TokenType.Comma:
            {
                return right;
            }
            default:
                throw new InvalidOperationException($"Unsupported binary operator: '{op.Type}'");
        }

        void CheckOperandsAreNumbers(ShimmerValue leftValue, ShimmerValue rightValue)
        {
            if (!leftValue.IsNumber || !rightValue.IsNumber)
                throw UnsupportedOperandTypeBinary(op, leftValue.Type, rightValue.Type);
        }
    }

    private ShimmerValue EvalConditionalExpr(ConditionalExpr conditionalExpr) =>
        IsTruthy(Eval(conditionalExpr.Condition)) ? Eval(conditionalExpr.ThenExpr) : Eval(conditionalExpr.ElseExpr);
    
    private ShimmerValue EvalUnaryExpr(UnaryExpr unaryExpr)
    {
        var op = unaryExpr.Op;
        var operand = Eval(unaryExpr.Expr);

        return op.Type switch
        {
            TokenType.Minus => operand.IsNumber
                ? ShimmerValue.Number(-operand.AsNumber)
                : throw UnsupportedOperandTypeUnary(op, operand.Type),

            TokenType.Bang => IsFalsy(operand) ? ShimmerValue.True : ShimmerValue.False,

            _ => throw new InvalidOperationException($"Unsupported unary operator: '{op.Type}'")
        };
    }

    private ShimmerValue EvalVarExpr(VarExpr varExpr) => _environment.Get(varExpr.Name);

    private static bool IsFalsy(ShimmerValue value) => value.IsNil || value is { IsBool: true, AsBool: false };
    private static bool IsTruthy(ShimmerValue value) => !IsFalsy(value);

    private static RuntimeError UnsupportedOperandTypeUnary(Token op, ShimmerType argType) =>
        RuntimeError.Create(op, $"Bad operand type for unary '{op.Lexeme}': '{argType}'.");

    private static RuntimeError UnsupportedOperandTypeBinary(Token op, ShimmerType left, ShimmerType right) =>
        RuntimeError.Create(op, $"Unsupported operand type(s) for '{op.Lexeme}': '{left}' and '{right}'.");
}