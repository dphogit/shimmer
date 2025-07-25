﻿namespace Shimmer.UnitTests.Helpers;

public static class TextWriterExtensions
{
    public static void AssertOutput(this TextWriter writer, string expected)
    {
        Assert.Equal(expected, writer.ToString()?.Trim());
    }

    public static void AssertRuntimeError(this TextWriter writer, int line, string message)
    {
        var s = writer.ToString();
        
        Assert.NotNull(s);
        s.AssertRuntimeError(line, message);
    }

    public static void AssertRuntimeError(this string s, int line, string message)
    {
        var expected = $"[Line {line}] Runtime error: {message}";
        Assert.Equal(expected, s.Trim());
    }
}