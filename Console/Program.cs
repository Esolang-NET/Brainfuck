using Brainfuck;
using System.Buffers;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Pipelines;
using System.Text;

var rootCommand = new RootCommand("run brainfuck from source code");

BrainfuckOptionBinder option;
{
    var noUseDefaultValue = new Option<bool>(new[] { "--syntax-no-use-default-value", "-snd" },() => false, "not use brainfuck option default value.");
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
    option = new(
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
{
    var sourceArgument = new Argument<string>("source", "brainfuck source");
    rootCommand.AddArgument(sourceArgument);
    rootCommand.SetHandler(async (InvocationContext context) =>
    {
        var source = context.ParseResult.GetValueForArgument(sourceArgument);
        var console = context.Console;
        var o = GetValueForHandlerParameter(option, context);
        var sequence = new BrainfuckSequenceEnumerable(source, o).Select(v => v.Sequence).ToArray().AsMemory();
        var input = new Pipe();
        var output = new Pipe();

        var cancellationToken = context.GetCancellationToken();
        var encoding = Encoding.UTF8;
        var runner = new BrainfuckRunner(source: source, sourceOptions: o, output: output.Writer, input: input.Reader);
        foreach (var command in runner.RunStep())
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
                return Convert.ToChar(Console.Read());
            if (Console.KeyAvailable == false)
                await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
            var key = Console.ReadKey(true);
            return key.KeyChar;
        }
    });
}
var parseCommand = new Command("parse", "parse brainfuck source code.");
{

    var sourceArgument = new Argument<string>("source", "brainfuck source");
    parseCommand.AddArgument(sourceArgument);
    parseCommand.SetHandler((context) => {

        var console = context.Console;
        var o = GetValueForHandlerParameter(option, context);
        var source = context.ParseResult.GetValueForArgument(sourceArgument);
        foreach (var (sequence, syntaxes) in new BrainfuckSequenceEnumerable(source, o))
        {
            Console.WriteLine($"{sequence}: {syntaxes}");
        }
        context.ExitCode = 0;
        return;
    });
}
rootCommand.AddCommand(parseCommand);

var builder = new CommandLineBuilder(rootCommand);
builder.UseDefaults();
builder.UseHelp();
var app = builder.Build();
await app.InvokeAsync(args);

static T? GetValueForHandlerParameter<T>(
        IValueDescriptor<T> symbol,
        InvocationContext context)
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
        _ => throw new ArgumentOutOfRangeException()
    };
}
class BrainfuckOptionBinder : BinderBase<IBrainfuckOptions>
{
    public BrainfuckOptionBinder(
    Option<bool> noUseDefaultValue,
    Option<string?> incrementPointer,
    Option<string?> decrementPointer,
    Option<string?> incrementCurrent,
    Option<string?> decrementCurrent,
    Option<string?> output,
    Option<string?> input,
    Option<string?> begin,
    Option<string?> end)
        => (NoUseDefaultValue, IncrementPointer, DecrementPointer, IncrementCurrent, DecrementCurrent, Output, Input, Begin, End) = (noUseDefaultValue, incrementPointer, decrementPointer, incrementCurrent, decrementCurrent, output, input, begin, end);
    readonly Option<bool> NoUseDefaultValue;
    /// <summary>
    /// Increment the data pointer by one (to point to the next cell to the right).
    /// </summary>
    readonly Option<string?> IncrementPointer;
    /// <summary>
    /// Decrement the data pointer by one (to point to the next cell to the left).
    /// </summary>
    readonly Option<string?> DecrementPointer;
    /// <summary>
    /// Increment the byte at the data pointer by one.
    /// </summary>
    readonly Option<string?> IncrementCurrent;
    /// <summary>
    /// Decrement the byte at the data pointer by one.
    /// </summary>
    readonly Option<string?> DecrementCurrent;
    /// <summary>
    /// Output the byte at the data pointer.
    /// </summary>
    readonly Option<string?> Output;
    /// <summary>
    /// Accept one byte of input, storing its value in the byte at the data pointer.
    /// </summary>
    readonly Option<string?> Input;
    /// <summary>
    /// If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
    /// </summary>
    readonly Option<string?> Begin;
    /// <summary>
    /// If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.
    /// </summary>
    readonly Option<string?> End;
    record BrainfuckOptions(string IncrementPointer, string DecrementPointer, string IncrementCurrent, string DecrementCurrent, string Output, string Input, string Begin, string End) : IBrainfuckOptions;
    protected override IBrainfuckOptions GetBoundValue(BindingContext bindingContext)
    {
        var noUseDefaultValue = bindingContext.ParseResult.GetValueForOption(NoUseDefaultValue);
        var incrementPointer = bindingContext.ParseResult.GetValueForOption(IncrementPointer);
        if (!noUseDefaultValue && string.IsNullOrEmpty(incrementPointer))
            incrementPointer = BrainfuckOptionsDefault.IncrementPointer;
        var decrementPointer = bindingContext.ParseResult.GetValueForOption(DecrementPointer);
        if (!noUseDefaultValue && string.IsNullOrEmpty(decrementPointer))
            decrementPointer = BrainfuckOptionsDefault.DecrementPointer;
        var incrementCurrent = bindingContext.ParseResult.GetValueForOption(IncrementCurrent);
        if (!noUseDefaultValue && string.IsNullOrEmpty(incrementCurrent))
            incrementCurrent = BrainfuckOptionsDefault.IncrementCurrent;
        var decrementCurrent = bindingContext.ParseResult.GetValueForOption(DecrementCurrent);
        if (!noUseDefaultValue && string.IsNullOrEmpty(decrementCurrent))
            decrementCurrent = BrainfuckOptionsDefault.DecrementCurrent;
        var output = bindingContext.ParseResult.GetValueForOption(Output);
        if (!noUseDefaultValue && string.IsNullOrEmpty(output))
            output = BrainfuckOptionsDefault.Output;
        var input = bindingContext.ParseResult.GetValueForOption(Input);
        if (!noUseDefaultValue && string.IsNullOrEmpty(input))
            input = BrainfuckOptionsDefault.Input;
        var begin = bindingContext.ParseResult.GetValueForOption(Begin);
        if (!noUseDefaultValue && string.IsNullOrEmpty(begin))
            begin = BrainfuckOptionsDefault.Begin;
        var end = bindingContext.ParseResult.GetValueForOption(End);
        if (!noUseDefaultValue && string.IsNullOrEmpty(end))
            end = BrainfuckOptionsDefault.End;
        return new BrainfuckOptions(
            IncrementPointer:incrementPointer!,
            DecrementPointer:decrementPointer!,
            IncrementCurrent:incrementCurrent!,
            DecrementCurrent: decrementCurrent!,
            Output:output!,
            Input:input!,
            Begin:begin!,
            End:end!
        );
    }
}
