using Brainfuck.Console;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

RootCommand rootCommand = new();

var option = rootCommand.AddDefaultGlobalOptions();
rootCommand
    .AddDefaultCommand(option)
    .AddParseCommand(option);
var app = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHelp()
    .Build();
await app.InvokeAsync(args);

