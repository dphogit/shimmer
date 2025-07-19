using Shimmer.Interpreter;
using Shimmer.Representation;
using Shimmer.Scanning;
using Shimmer.UnitTests.Helpers;
using Environment = Shimmer.Interpreter.Environment;

namespace Shimmer.UnitTests.Interpreter;

public class EnvironmentTests
{
    private readonly TokenFactory _tokenFactory = new();
    
    [Fact]
    public void Define_NewVariable_Binds()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        var env = new Environment();
        
        // Act
        env.Define(name, ShimmerValue.Number(1));
        
        // Assert
        var value = env.Get(name);
        Assert.Equal(1, value.AsNumber);
    }

    [Fact]
    public void Define_ExistingVariable_ThrowsRuntimeError()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        
        var env = new Environment();
        env.Define(name, ShimmerValue.Number(1));
        
        // Act
        var action = () => env.Define(name, ShimmerValue.Number(2));
        
        // Assert
        var error = Assert.Throws<RuntimeError>(action);
        error.Message.AssertRuntimeError(1, $"Variable '{name.Lexeme}' already defined in this scope.");
    }

    [Fact]
    public void Get_DefinedVariable_ReturnsValue()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        
        var env = new Environment();
        env.Define(name, ShimmerValue.Number(1));
        
        // Act
        var value = env.Get(name);
        
        // Assert
        Assert.Equal(1, value.AsNumber);
    }

    [Fact]
    public void Get_DefinedVariableInEnclosing_ReturnsValue()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");

        var enclosingEnv = new Environment();
        enclosingEnv.Define(name, ShimmerValue.Number(1));

        var env = new Environment(enclosingEnv);
        
        // Act
        var value = env.Get(name);
        
        // Assert
        Assert.Equal(1, value.AsNumber);
    }

    [Fact]
    public void Get_UndefinedVariable_ThrowsRuntimeError()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        
        var env = new Environment();
        
        // Act
        var action = () => env.Get(name);
        
        // Assert
        var error = Assert.Throws<RuntimeError>(action);
        error.Message.AssertRuntimeError(1, $"Undefined variable '{name.Lexeme}'.");
    }

    [Fact]
    public void Assign_DefinedVariable_UpdatesValue()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        
        var env = new Environment();
        env.Define(name, ShimmerValue.Number(1));
        
        // Act
        env.Assign(name, ShimmerValue.Number(2));
        
        // Assert
        var value = env.Get(name);
        Assert.Equal(2, value.AsNumber);
    }
    
    [Fact]
    public void Assign_UndefinedVariable_ThrowsRuntimeError()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        
        var env = new Environment();
        
        // Act
        var action = () => env.Assign(name, ShimmerValue.Number(1));
        
        // Assert
        var error = Assert.Throws<RuntimeError>(action);
        error.Message.AssertRuntimeError(1, $"Undefined variable '{name.Lexeme}'.");
    }

    [Fact]
    public void Assign_DefinedVariableInEnclosing_UpdatesValue()
    {
        // Arrange
        var name = _tokenFactory.Identifier("x");
        
        var enclosingEnv = new Environment();
        enclosingEnv.Define(name, ShimmerValue.Number(1));
        
        var env = new Environment(enclosingEnv);
        
        // Act
        env.Assign(name, ShimmerValue.Number(2));
        
        // Assert
        var value = env.Get(name);
        Assert.Equal(2, value.AsNumber);
    }
}