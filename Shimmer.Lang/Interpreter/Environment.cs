using Shimmer.Representation;
using Shimmer.Representation.Functions.Native;
using Shimmer.Scanning;

namespace Shimmer.Interpreter;

/// <summary>
/// Stores the bindings that associate variables with values.
/// Environments can chain, pointing to their parent if a local scope environment.
/// </summary>
public class Environment
{
    private readonly Environment? _enclosing = null;
    private readonly Dictionary<string, ShimmerValue> _values;

    public Environment() : this(null)
    {
    }

    public Environment(Environment? enclosing)
    {
        _enclosing = enclosing;
        _values = new Dictionary<string, ShimmerValue>();
    }

    public static Environment CreateGlobal() => new Environment().AddNatives();

    /// <summary>
    /// Bind the value to the given variable in this environment. Throws a <see cref="RuntimeError"/> if a
    /// variable with the same name is already defined in this environment. The runtime error should only occur for
    /// global scoped variables, local variable redefinition should be detected and prevented in the resolving pass.
    /// variables if detected/prevented correctly in the resolving pass.
    /// </summary>
    /// <param name="name">The token containing the lexeme which is the variable name.</param>
    /// <param name="value">The value to assign to the variable.</param>
    /// <exception cref="RuntimeError"></exception>
    public void Define(Token name, ShimmerValue value)
    {
        if (!_values.TryAdd(name.Lexeme, value))
            throw RuntimeError.Create(name, $"Variable '{name.Lexeme}' already defined in this scope.");
    }

    /// <summary>
    /// Get the value for the given variable in this or enclosing environment.
    /// Throws a <see cref="RuntimeError"/> if the variable is not defined.
    /// </summary>
    /// <param name="name">The token containing the lexeme which is the variable name.</param>
    /// <returns>The <see cref="ShimmerValue"/> associated to the variable.</returns>
    /// <exception cref="RuntimeError"></exception>
    public ShimmerValue Get(Token name)
    {
        var current = this;

        while (current is not null)
        {
            if (current._values.TryGetValue(name.Lexeme, out var value))
                return value;
            
            current = current._enclosing;
        }
        
        throw UndefinedVariableRuntimeError(name);
    }

    /// <summary>
    /// Gets the value for the given variable at the ancestor environment located <paramref name="distance"/>
    /// environments above this one.
    /// </summary>
    /// <param name="name">The token containing the lexeme which is the variable name.</param>
    /// <param name="distance">The number of ancestors to traverse up from this environment.</param>
    /// <returns></returns>
    /// <exception cref="RuntimeError">Variable is not defined at ancestor environment.</exception>
    /// <exception cref="ArgumentException">Distance exceeds the number of ancestors.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Negative value.</exception>
    public ShimmerValue GetAt(Token name, int distance)
    {
        return GetAncestor(distance)._values.TryGetValue(name.Lexeme, out var value)
            ? value
            : throw UndefinedVariableRuntimeError(name);
    }

    /// <summary>
    /// Assigns the value to the variable. The variable must be defined in this or enclosing environments.
    /// Otherwise, throws a <see cref="RuntimeError"/> if the variable is not defined.
    /// </summary>
    /// <param name="name">The token containing the lexeme which is the variable name.</param>
    /// <param name="value">The value to assign to the variable.</param>
    /// <exception cref="RuntimeError"></exception>
    public void Assign(Token name, ShimmerValue value)
    {
        var current = this;

        while (current is not null)
        {
            if (current._values.ContainsKey(name.Lexeme))
            {
                current._values[name.Lexeme] = value;
                return;
            }
            
            current = current._enclosing;
        }

        throw UndefinedVariableRuntimeError(name);
    }

    /// <summary>
    /// Assigns the value for the given variable at the ancestor environment located <paramref name="distance"/>
    /// environments above this one.
    /// </summary>
    /// <param name="name">The token containing the lexeme which is the variable name.</param>
    /// <param name="value">The value to assign to the variable.</param>
    /// <param name="distance">The number of ancestors to traverse up from this environment.</param>
    /// <returns></returns>
    /// <exception cref="RuntimeError">Variable is not defined at ancestor environment.</exception>
    /// <exception cref="ArgumentException">Distance exceeds the number of ancestors.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Negative value.</exception>
    public void AssignAt(Token name, ShimmerValue value, int distance)
    {
        var ancestor = GetAncestor(distance);

        if (!ancestor._values.ContainsKey(name.Lexeme))
            throw UndefinedVariableRuntimeError(name);
        
        ancestor._values[name.Lexeme] = value;
    }
    
    private Environment GetAncestor(int distance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(distance, nameof(distance));

        var current = this;

        for (var i = 0; i < distance; i++)
        {
            current = current._enclosing ??
                      throw new ArgumentException("Distance exceeds number of ancestors.", nameof(distance));
        }
        
        return current;
    }

    private static RuntimeError UndefinedVariableRuntimeError(Token name) =>
        RuntimeError.Create(name, $"Undefined variable '{name.Lexeme}'.");
}