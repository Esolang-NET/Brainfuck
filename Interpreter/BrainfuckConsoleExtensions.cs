using Esolang.Brainfuck.Processor;
using Esolang.Interpreter;
using System.CommandLine;


namespace Esolang.Brainfuck.Interpreter;

/// <summary>
/// Extension methods that compose Brainfuck CLI commands.
/// </summary>
public static class BrainfuckInterpreterExtensions
{
    /// <summary>
    /// Adds a command that executes Brainfuck code with the specified syntax options.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="command"></param>
    /// <param name="binder"></param>
    /// <returns></returns>
    public static T AddBrainfuckCommand<T>(this T command, out BrainfuckOptionBinder binder)
        where T : Command
    {
        binder = command.AddDefaultGlobalOptions();
        return command
                .AddDefaultCommand(binder)
                .AddParseCommand(binder);
    }
    /// <summary>
    /// Adds global options that represent Brainfuck syntax configuration.
    /// </summary>
    /// <param name="rootCommand">The target root command.</param>
    /// <returns>A binder that groups the added options.</returns>
    public static BrainfuckOptionBinder AddDefaultGlobalOptions<T>(this T rootCommand)
        where T : Command
    {
        var noUseDefaultValue = new Option<bool>("--syntax-no-use-default-value", "-snd")
        {
            Description = SR.Get("SyntaxNoUseDefaultValueDescription"),
            DefaultValueFactory = _ => false,
        };
        rootCommand.Options.Add(noUseDefaultValue);
        var incrementPointer = new Option<string?>("--syntax-increment-pointer", "-sip")
        {
            DefaultValueFactory = _ => null,
            Description = SR.Get("SyntaxIncrementPointerDescription"),
        };
        rootCommand.Options.Add(incrementPointer);
        var decrementPointer = new Option<string?>("--syntax-dencrement-pointer", "-sdp")
        {
            DefaultValueFactory = _ => null,
            Description = SR.Get("SyntaxDecrementPointerDescription"),
        };
        rootCommand.Options.Add(decrementPointer);
        var incrementCurrent = new Option<string?>("--syntax-increment-current", "-sic")
        {
            DefaultValueFactory = _ => null,
            Description = SR.Get("SyntaxIncrementCurrentDescription"),
        };
        rootCommand.Options.Add(incrementCurrent);
        var decrementCurrent = new Option<string?>("--syntax-decrement-current", "-sdc")
        {
            DefaultValueFactory = _ => null,
            Description = SR.Get("SyntaxDecrementCurrentDescription"),
        };
        rootCommand.Options.Add(decrementCurrent);
        var output = new Option<string?>("--syntax-output", "-so")
        {
            DefaultValueFactory = _ => null,
            Description = SR.Get("SyntaxOutputDescription"),
        };
        rootCommand.Options.Add(output);
        var input = new Option<string?>("--syntax-input", "-si")
        {
            DefaultValueFactory = _ => null,
            Description = SR.Get("SyntaxInputDescription")
        };
        rootCommand.Options.Add(input);
        var begin = new Option<string?>("--syntax-begin", "-sb") { DefaultValueFactory = _ => null, Description = SR.Get("SyntaxBeginDescription") };
        rootCommand.Options.Add(begin);
        var end = new Option<string?>("--syntax-end", "-se") { DefaultValueFactory = _ => null, Description = SR.Get("SyntaxEndDescription") };
        rootCommand.Options.Add(end);
        return new(
            noUseDefaultValue: noUseDefaultValue,
            incrementPointer: incrementPointer,
            decrementPointer: decrementPointer,
            incrementCurrent: incrementCurrent,
            decrementCurrent: decrementCurrent,
            output: output,
            input: input,
            begin: begin,
            end: end
        );
    }
    /// <summary>
    /// Configures the default execution command.
    /// </summary>
    /// <param name="rootCommand">The target root command.</param>
    /// <param name="option">The syntax option binder.</param>
    /// <returns>The configured root command.</returns>
    public static T AddDefaultCommand<T>(this T rootCommand, BrainfuckOptionBinder option)
        where T : Command
    {
        rootCommand.Description = SR.Get("RootCommandDescription");
        var sourceArgument = new Argument<string>("source")
        {
            Description = SR.Get("SourceArgumentDescription"),
        };
        rootCommand.Arguments.Add(sourceArgument);
        rootCommand.SetAction(async (result, cancellationToken) =>
        {
            var source = result.GetRequiredValue(sourceArgument);
            var o = option.GetValue(result);

            var processor = new BrainfuckProcessor(source: source, sourceOptions: o);

            return await processor.RunToConsoleAsync(cancellationToken);
        });
        return rootCommand;
    }
    /// <summary>
    /// Adds a subcommand that prints parse results.
    /// </summary>
    /// <param name="rootCommand">The target root command.</param>
    /// <param name="option">The syntax option binder.</param>
    /// <returns>The configured command.</returns>
    /// <typeparam name="T">The type of the root command.</typeparam>
    public static T AddParseCommand<T>(this T rootCommand, BrainfuckOptionBinder option)
        where T : Command
    {
        var parseCommand = new Command("parse", SR.Get("ParseCommandDescription"));

        var sourceArgument = new Argument<string>("source")
        {
            Description = SR.Get("SourceArgumentDescription"),
        };
        parseCommand.Arguments.Add(sourceArgument);
        parseCommand.SetAction((parseResult, cancellationToken) =>
        {

            var output = parseResult.InvocationConfiguration.Output;
            var o = option.GetValue(parseResult);
            var source = parseResult.GetRequiredValue(sourceArgument);
            foreach (var (sequence, syntaxes) in new BrainfuckSequenceEnumerable(source, o, cancellationToken))
            {
                output.WriteLine($"{sequence}: {syntaxes}");
            }
            return Task.FromResult(0);
        });
        rootCommand.Add(parseCommand);
        return rootCommand;
    }
}
