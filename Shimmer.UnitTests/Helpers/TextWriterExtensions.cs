namespace Shimmer.UnitTests.Helpers;

public static class TextWriterExtensions
{
    public static void AssertOutput(this TextWriter writer, string expected)
    {
        Assert.Equal(expected, writer.ToString()?.Trim());
    }
}