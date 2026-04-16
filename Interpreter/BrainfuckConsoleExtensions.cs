using Esolang.Brainfuck.Processor;
using System.Buffers;
using System.CommandLine;
using System.IO.Pipelines;
using System.Text;


namespace Esolang.Brainfuck.Interpreter;

/// <summary>
/// Extension methods that compose Brainfuck CLI commands.
/// </summary>
public static class BrainfuckInterpreterExtensions
{
    /// <summary>
    /// Adds global options that represent Brainfuck syntax configuration.
    /// </summary>
    /// <param name="rootCommand">The target root command.</param>
    /// <returns>A binder that groups the added options.</returns>
    public static BrainfuckOptionBinder AddDefaultGlobalOptions(this RootCommand rootCommand)
    {
        var noUseDefaultValue = new Option<bool>("--syntax-no-use-default-value", "-snd")
        {
            Description = "not use brainfuck option default value.",
            DefaultValueFactory = _ => false,
        };
        rootCommand.Options.Add(noUseDefaultValue);
        var incrementPointer = new Option<string?>("--syntax-increment-pointer", "-sip")
        {
            DefaultValueFactory = _ => null,
            Description = "Increment the data pointer by one (to point to the next cell to the right).",
        };
        rootCommand.Options.Add(incrementPointer);
        var decrementPointer = new Option<string?>("--syntax-dencrement-pointer", "-sdp")
        {
            DefaultValueFactory = _ => null,
            Description = "Decrement the data pointer by one (to point to the next cell to the left).",
        };
        rootCommand.Options.Add(decrementPointer);
        var incrementCurrent = new Option<string?>("--syntax-increment-current", "-sic") 
        { 
            DefaultValueFactory = _ => null, 
            Description = "Increment the byte at the data pointer by one.",
        };
        rootCommand.Options.Add(incrementCurrent);
        var decrementCurrent = new Option<string?>("--syntax-decrement-current", "-sdc") { 
            DefaultValueFactory = _ => null, 
            Description = "Decrement the byte at the data pointer by one.",
        };
        rootCommand.Options.Add(decrementCurrent);
        var output = new Option<string?>("--syntax-output", "-so") 
        {
            DefaultValueFactory = _ => null,
            Description = "Output the byte at the data pointer." ,
        };
        rootCommand.Options.Add(output);
        var input = new Option<string?>("--syntax-input", "-si") 
        { 
            DefaultValueFactory = _ => null, 
            Description = "Accept one byte of input, storing its value in the byte at the data pointer." 
        };
        rootCommand.Options.Add(input);
        var begin = new Option<string?>("--syntax-begin", "-sb") { DefaultValueFactory = _ => null, Description = "If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command." };
        rootCommand.Options.Add(begin);
        var end = new Option<string?>("--syntax-end", "-se") { DefaultValueFactory = _ => null, Description = "If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command." };
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
    public static RootCommand AddDefaultCommand(this RootCommand rootCommand, BrainfuckOptionBinder option)
    {
        rootCommand.Description = "run brainfuck from source code";
        var sourceArgument = new Argument<string>("source")
        {
            Description = "brainfuck source",
        };
        rootCommand.Arguments.Add(sourceArgument);
        rootCommand.SetAction(async (result, cancellationToken) =>
        {
            var source = result.GetRequiredValue(sourceArgument);
            var console = Console.In;
            var o = option.GetValue(result);
            var sequence = new BrainfuckSequenceEnumerable(source, o).Select(v => v.Sequence).ToArray().AsMemory();
            var input = new Pipe();
            var output = new Pipe();

            var encoding = Encoding.UTF8;
            var runner = new BrainfuckProcessor(source: source, sourceOptions: o, output: output.Writer, input: input.Reader);
            foreach (var command in runner.StepCommands())
            {
                if (command.RequiredInput)
                {
                    var kc = await ConsoleReadAsync(console, cancellationToken);
                    var bytes = encoding.GetBytes(new[] { kc });
                    var flush = await input.Writer.WriteAsync(bytes, cancellationToken);
                    await input.Writer.FlushAsync(cancellationToken);

                }
                await command.ExecuteAsync(cancellationToken);
                if (command.RequiredOutput)
                {
                    var reader = await output.Reader.ReadAsync(cancellationToken);
                    var buffer = reader.Buffer;
                    if (buffer.Length > 0)
                    {
                        var sequence2 = buffer.Slice(buffer.Start, buffer.End);
                        if (encoding.GetString(sequence2.FirstSpan) is string chars && !string.IsNullOrEmpty(chars))
                        {
                            if (!Console.IsOutputRedirected && chars == "\r") chars = Environment.NewLine;
                            result.InvocationConfiguration.Output.Write(chars);
                        }

                        output.Reader.AdvanceTo(buffer.End);
                    }
                }
            }
            return 0;
            static async ValueTask<char> ConsoleReadAsync(TextReader console, CancellationToken cancellationToken)
            {
                if (Console.IsInputRedirected)
                    return Convert.ToChar(System.Console.Read());
                if (System.Console.KeyAvailable == false)
                    await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
                var key = System.Console.ReadKey(true);
                return key.KeyChar;
            }
        });
        return rootCommand;
    }
    /// <summary>
    /// Adds a subcommand that prints parse results.
    /// </summary>
    /// <param name="rootCommand">The target root command.</param>
    /// <param name="option">The syntax option binder.</param>
    /// <returns>The configured root command.</returns>
    public static RootCommand AddParseCommand(this RootCommand rootCommand, BrainfuckOptionBinder option)
    {
        var parseCommand = new Command("parse", "parse brainfuck source code.");

        var sourceArgument = new Argument<string>("source")
        {
            Description = "brainfuck source",
        };
        parseCommand.Arguments.Add(sourceArgument);
        parseCommand.SetAction(parseResult =>
        {

            var output = parseResult.InvocationConfiguration.Output;
            var o = option.GetValue(parseResult);
            var source = parseResult.GetRequiredValue(sourceArgument);
            foreach (var (sequence, syntaxes) in new BrainfuckSequenceEnumerable(source, o))
            {
                output.WriteLine($"{sequence}: {syntaxes}");
            }
            return 0;
        });
        rootCommand.Add(parseCommand);
        return rootCommand;
    }
}
