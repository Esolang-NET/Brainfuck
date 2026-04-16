using Esolang.Brainfuck.Interpreter;
using System.CommandLine;

RootCommand rootCommand = new();

var option = rootCommand.AddDefaultGlobalOptions();
rootCommand
    .AddDefaultCommand(option)
    .AddParseCommand(option);
await rootCommand.Parse(args).InvokeAsync();

