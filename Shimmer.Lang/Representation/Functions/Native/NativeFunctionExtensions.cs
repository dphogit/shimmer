using System.Reflection;
using Shimmer.Scanning;
using Environment = Shimmer.Interpreter.Environment;

namespace Shimmer.Representation.Functions.Native;

public static class NativeFunctionExtensions
{
    public static Environment AddNatives(this Environment environment)
    {
        var nativeTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(NativeFunction).IsAssignableFrom(t));

        foreach (var type in nativeTypes)
        {
            if (Activator.CreateInstance(type) is NativeFunction nativeFunction)
                environment.DefineNative(nativeFunction);
        }
        
        return environment;
    }

    private static void DefineNative(this Environment environment, NativeFunction nativeFunction)
    {
        var name = new Token { Lexeme = nativeFunction.Name, Line = 0, Column = 0, Type = TokenType.Identifier };
        environment.Define(name, ShimmerValue.Function(nativeFunction));
    }
}