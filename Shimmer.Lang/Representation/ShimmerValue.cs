﻿using System.Diagnostics;
using System.Globalization;
using Shimmer.Representation.Functions;

namespace Shimmer.Representation;

public class ShimmerValue
{
    public ShimmerType Type { get; }
    private object? Value { get; }

    private ShimmerValue(ShimmerType type, object? value)
    {
        Type = type;
        Value = value;
    }
    
    // We only need one instance of values rather than creating a new object everytime
    public static readonly ShimmerValue True = new(ShimmerType.Bool, true);
    public static readonly ShimmerValue False = new(ShimmerType.Bool, false);
    public static readonly ShimmerValue Nil = new(ShimmerType.Nil, null);

    public static ShimmerValue Number(double d) => new(ShimmerType.Number, d);
    public bool IsNumber => Type == ShimmerType.Number;
    public double AsNumber =>
        IsNumber ? (double)Value! : throw new InvalidOperationException("Value is not a number.");

    public static ShimmerValue Bool(bool b) => b ? True : False;
    public bool IsBool => Type == ShimmerType.Bool;
    public bool AsBool => IsBool ? (bool)Value! : throw new InvalidOperationException("Value is not a boolean.");
    
    public static ShimmerValue String(string s) => new(ShimmerType.String, s);
    public bool IsString => Type == ShimmerType.String;
    public string AsString => IsString ? (string)Value! : throw new InvalidOperationException("Value is not a string.");

    public static ShimmerValue Function(ShimmerFunction f) => new(ShimmerType.Function, f);
    public bool IsFunction => Type == ShimmerType.Function;
    public ShimmerFunction AsFunction => IsFunction
        ? (ShimmerFunction)Value!
        : throw new InvalidOperationException("Value is not a function.");

    public bool IsNil => Type == ShimmerType.Nil;
    
    public bool IsCallable => Type == ShimmerType.Function;
    
    public override string ToString() =>
        Type switch
        {
            ShimmerType.Number => AsNumber.ToString(CultureInfo.InvariantCulture)!,
            ShimmerType.Bool => AsBool ? "true" : "false",
            ShimmerType.Nil => "nil",
            ShimmerType.String => $"\"{AsString}\"",
            ShimmerType.Function => AsFunction.ToString()!,
            _ => throw new UnreachableException($"Unknown shimmer value type '{Type}'."),
        };

    public bool Equals(ShimmerValue other)
    {
        const double tolerance = 1e-9;
        
        if (Type != other.Type)
            return false;

        return Type switch
        {
            ShimmerType.Number => Math.Abs(AsNumber - other.AsNumber) < tolerance,
            ShimmerType.Bool => AsBool == other.AsBool,
            ShimmerType.Nil => other.IsNil,
            ShimmerType.String => AsString == other.AsString,
            ShimmerType.Function => AsFunction == other.AsFunction,
            _ => throw new UnreachableException($"Unknown shimmer value type '{Type}'."),
        };
    }
}