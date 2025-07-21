using Shimmer;

var driver = new ShimmerDriver();

switch (args.Length)
{
    case 0:
        Repl();
        return (int)ExitCode.Success;
    case 1:
        return (int)RunFile(args[0]);
    default:
        Console.WriteLine("Usage: shimmer [file]");
        return (int)ExitCode.Failure;
}

void Repl()
{
    Console.WriteLine("Shimmer v0.0.1 (ALPHA)");

    while (true)
    {
        Console.Write("> ");
        var source = Console.ReadLine();

        if (source is null)
            return;

        driver.Run(source);
    }
}

ExitCode RunFile(string path) => driver.RunFile(path) ? ExitCode.Success : ExitCode.Failure;

internal enum ExitCode
{
    Success = 0,
    Failure = 1,
}
