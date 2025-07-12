namespace Shimmer.Errors;

public interface IErrorReporter
{
    void ReportError(string message);
}