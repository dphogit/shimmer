namespace Shimmer.Errors;

public class ConsoleErrorReporter : IErrorReporter
{
    public void ReportError(string message)
    {
        Console.WriteLine(message);
    }
}