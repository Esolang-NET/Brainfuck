using Esolang.Brainfuck.Interpreter;
using System.CommandLine;

CancellationTokenSource cancellationTokenSource = new();
void CancelKeyPress(object? _, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
}
try
{
    Console.CancelKeyPress += CancelKeyPress;
    return await RunAsync(args, cancellationTokenSource.Token);
}
finally
{
    Console.CancelKeyPress -= CancelKeyPress;
}

/// <summary>
/// The main program class for the Brainfuck interpreter. This class is responsible for setting up the command-line interface and handling user input. It defines the entry point of the application and orchestrates the execution of commands based on the provided arguments.
/// </summary>
internal partial class Program
{
    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        RootCommand rootCommand = [];
        var option = rootCommand.AddDefaultGlobalOptions();
        rootCommand
            .AddDefaultCommand(option)
            .AddParseCommand(option);
        return await rootCommand.Parse(args).InvokeAsync(cancellationToken: cancellationToken);

    }
}
