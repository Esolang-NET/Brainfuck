using Esolang.Brainfuck.Interpreter;
using System.CommandLine;

RootCommand rootCommand = [];
var option = rootCommand.AddDefaultGlobalOptions();
rootCommand
    .AddDefaultCommand(option)
    .AddParseCommand(option);
return await rootCommand.Parse(args).InvokeAsync();


/// <summary>
/// The main program class for the Brainfuck interpreter. This class is responsible for setting up the command-line interface and handling user input. It defines the entry point of the application and orchestrates the execution of commands based on the provided arguments.
/// </summary>
public partial class Program { }
