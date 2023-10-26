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
    .Build();
await app.InvokeAsync(args);

