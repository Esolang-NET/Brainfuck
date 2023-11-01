using Brainfuck.Runner;
using System.Buffers;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.IO.Pipelines;
using System.Text;


namespace Brainfuck.Console;

public static class BrainfuckConsoleExtensions
{
    public static BrainfuckOptionBinder AddDefaultGlobalOptions(this RootCommand rootCommand)
    {
        var noUseDefaultValue = new Option<bool>(new[] { "--syntax-no-use-default-value", "-snd" }, () => false, "not use brainfuck option default value.");
        rootCommand.AddGlobalOption(noUseDefaultValue);
        var incrementPointer = new Option<string?>(new[] { "--syntax-increment-pointer", "-sip" }, () => null, "Increment the data pointer by one (to point to the next cell to the right).");
        rootCommand.AddGlobalOption(incrementPointer);
        var decrementPointer = new Option<string?>(new[] { "--syntax-dencrement-pointer", "-sdp" }, () => null, "Decrement the data pointer by one (to point to the next cell to the left).");
        rootCommand.AddGlobalOption(decrementPointer);
        var incrementCurrent = new Option<string?>(new[] { "--syntax-increment-current", "-sic" }, () => null, "Increment the byte at the data pointer by one.");
        rootCommand.AddGlobalOption(decrementPointer);
        var decrementCurrent = new Option<string?>(new[] { "--syntax-decrement-current", "-sdc" }, () => null, "Decrement the byte at the data pointer by one.");
        rootCommand.AddGlobalOption(decrementCurrent);
        var output = new Option<string?>(new[] { "--syntax-output", "-so" }, () => null, "Output the byte at the data pointer.");
        rootCommand.AddGlobalOption(output);
        var input = new Option<string?>(new[] { "--syntax-input", "-si" }, () => null, "Accept one byte of input, storing its value in the byte at the data pointer.");
        rootCommand.AddGlobalOption(input);
        var begin = new Option<string?>(new[] { "--syntax-begin", "-sb" }, () => null, "If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.");
        rootCommand.AddGlobalOption(begin);
        var end = new Option<string?>(new[] { "--syntax-end", "-se" }, () => null, "If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.");
        rootCommand.AddGlobalOption(end);
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
    public static T? GetValueForHandlerParameter<T>(
        this InvocationContext context,
        IValueDescriptor<T> symbol
    )
    {
        if (symbol is IValueSource valueSource &&
            valueSource.TryGetValue(symbol, context.BindingContext, out var boundValue) &&
            boundValue is T value)
        {
            return value;
        }
        else
        {
            return GetValueFor(context.ParseResult, symbol);
        }
        static T? GetValueFor(ParseResult result, IValueDescriptor<T> symbol) =>
        symbol switch
        {
            Argument<T> argument => result.GetValueForArgument(argument),
            Option<T> option => result.GetValueForOption(option),
            _ => throw new ArgumentOutOfRangeException(nameof(symbol))
        };
    }
    public static RootCommand AddDefaultCommand(this RootCommand rootCommand, BrainfuckOptionBinder option)
    {
        rootCommand.Description = "run brainfuck from source code";
        var sourceArgument = new Argument<string>("source", "brainfuck source");
        rootCommand.AddArgument(sourceArgument);
        rootCommand.SetHandler(async (InvocationContext context) =>
        {
            var source = context.ParseResult.GetValueForArgument(sourceArgument);
            var console = context.Console;
            var o = context.GetValueForHandlerParameter(option);
            var sequence = new BrainfuckSequenceEnumerable(source, o).Select(v => v.Sequence).ToArray().AsMemory();
            var input = new Pipe();
            var output = new Pipe();

            var cancellationToken = context.GetCancellationToken();
            var encoding = Encoding.UTF8;
            var runner = new BrainfuckRunner(source: source, sourceOptions: o, output: output.Writer, input: input.Reader);
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
                            if (!console.IsOutputRedirected && chars == "\r") chars = Environment.NewLine;
                            console.Write(chars);
                        }

                        output.Reader.AdvanceTo(buffer.End);
                    }
                }
            }
            context.ExitCode = 0;
            return;
            static async ValueTask<char> ConsoleReadAsync(IConsole console, CancellationToken cancellationToken)
            {
                if (console.IsInputRedirected)
                    return Convert.ToChar(System.Console.Read());
                if (System.Console.KeyAvailable == false)
                    await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
                var key = System.Console.ReadKey(true);
                return key.KeyChar;
            }
        });
        return rootCommand;
    }
    public static RootCommand AddParseCommand(this RootCommand rootCommand, BrainfuckOptionBinder option)
    {
        var parseCommand = new Command("parse", "parse brainfuck source code.");

        var sourceArgument = new Argument<string>("source", "brainfuck source");
        parseCommand.AddArgument(sourceArgument);
        parseCommand.SetHandler((InvocationContext context) =>
        {

            var console = context.Console;
            var o = context.GetValueForHandlerParameter(option);
            var source = context.ParseResult.GetValueForArgument(sourceArgument);
            foreach (var (sequence, syntaxes) in new BrainfuckSequenceEnumerable(source, o))
            {
                console.Out.WriteLine($"{sequence}: {syntaxes}");
            }
            context.ExitCode = 0;
            return;
        });
        rootCommand.AddCommand(parseCommand);
        return rootCommand;
    }
}
