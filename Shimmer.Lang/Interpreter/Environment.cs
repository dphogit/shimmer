using Shimmer.Representation;
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

    /// <summary>
    /// Bind the value to the given variable in this environment.
    /// Throws a <see cref="RuntimeError"/> if a variable with the same name is
    /// already defined in this environment.
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
        
        throw RuntimeError.Create(name, $"Undefined variable '{name.Lexeme}'.");
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
                _values[name.Lexeme] = value;
                return;
            }
            
            current = current._enclosing;
        }

        throw RuntimeError.Create(name, $"Undefined variable '{name.Lexeme}'.");
    }
}