using System.CommandLine;

namespace Esolang.Brainfuck.Interpreter;

/// <summary>
/// Entry points for the dotnet-brainfuck command-line tool.
/// </summary>
public static class Program
{
    /// <summary>
    /// Runs the command-line pipeline and returns the process exit code.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>The exit code.</returns>
    public static async Task<int> RunAsync(string[] args)
    {
        RootCommand rootCommand = new();
        var option = rootCommand.AddDefaultGlobalOptions();
        rootCommand
            .AddDefaultCommand(option)
            .AddParseCommand(option);
        return await rootCommand.Parse(args).InvokeAsync();
    }

    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static async Task Main(string[] args)
        => _ = await RunAsync(args);
}
