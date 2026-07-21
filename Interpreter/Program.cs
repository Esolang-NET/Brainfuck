using Esolang.Brainfuck.Interpreter;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

return await RunAsync(args);

/// <summary>
/// The main program class for the Brainfuck interpreter. This class is responsible for setting up the command-line interface and handling user input. It defines the entry point of the application and orchestrates the execution of commands based on the provided arguments.
/// </summary>
partial class Program
{
    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        RootCommand rootCommand = [];
        return await rootCommand
            .AddBrainfuckCommand(out _)
            .Parse(args)
            .InvokeAsync(cancellationToken: cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    static async Task<int> RunAsync(string[] args)
    {
        try
        {
            Console.CancelKeyPress += CancelKeyPress;
            return await RunAsync(args, cancellationTokenSource.Token);
        }
        finally
        {
            Console.CancelKeyPress -= CancelKeyPress;
        }
    }
    static readonly CancellationTokenSource cancellationTokenSource = new();

    [ExcludeFromCodeCoverage]
    static void CancelKeyPress(object? _, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        cancellationTokenSource.Cancel();
    }
}
