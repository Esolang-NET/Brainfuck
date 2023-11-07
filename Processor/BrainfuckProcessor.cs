using Esolang.Brainfuck.Processor.SequenceCommands;
using System.Buffers;
using System.Collections.Immutable;
using System.IO.Pipelines;
using System.Text;

namespace Esolang.Brainfuck.Processor;

/// <summary>
/// 
/// </summary>
public sealed partial class BrainfuckProcessor
{
    readonly ReadOnlyMemory<BrainfuckSequence> Sequences;
    readonly PipeReader? Input;
    readonly PipeWriter? Output;
    BrainfuckContext Context => new(Sequences, SequencesIndex: 0, Stack: ImmutableArray.Create<byte>(0), StackIndex: 0, Input: Input, Output: Output);
    public BrainfuckProcessor(string source, PipeWriter? output = default, PipeReader? input = default) : this(source, new(), output, input) { }
    public BrainfuckProcessor(string source, IBrainfuckOptions? sourceOptions, PipeWriter? output = default, PipeReader? input = default)
        : this(SourceToSequences(source, sourceOptions), output: output, input: input) { }
    public BrainfuckProcessor(string source, BrainfuckOptions sourceOptions, PipeWriter? output = default, PipeReader? input = default)
        : this(SourceToSequences(source, sourceOptions), output: output, input: input) { }
    static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, IBrainfuckOptions? sourceOptions)
        => new BrainfuckSequenceEnumerable(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();
    static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, BrainfuckOptions sourceOptions)
        => new BrainfuckSequenceEnumerable(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();
    public BrainfuckProcessor(ReadOnlyMemory<BrainfuckSequence> sequences, PipeWriter? output = default, PipeReader? input = default)
        => (Sequences, Input, Output) = (sequences, input, output);

    public void Deconstruct(out ReadOnlyMemory<BrainfuckSequence> sequences, out PipeWriter? output, out PipeReader? input)
        => (sequences, input, output) = (Sequences, Input, Output);
    public BrainfuckContext Run(CancellationToken cancellationToken = default) => Run(null, cancellationToken);
    public BrainfuckContext Run(BrainfuckContext? context = null, CancellationToken cancellationToken = default) => InternalRun(context ?? Context, cancellationToken);
    public ValueTask<BrainfuckContext> RunAsync(CancellationToken cancellationToken = default) => RunAsync(null, cancellationToken);
    public ValueTask<BrainfuckContext> RunAsync(BrainfuckContext? context, CancellationToken cancellationToken = default) => InternalRunAsync(context ?? Context, cancellationToken);
    public IEnumerable<SequenceCommand> StepCommands() => InternalStepCommands(Context);
    static async ValueTask<BrainfuckContext> InternalRunAsync(BrainfuckContext context, CancellationToken cancellationToken = default)
    {
        var lastContext = context;
        foreach (var command in InternalStepCommands(context))
        {
            lastContext = await command.ExecuteAsync(cancellationToken);
        }
        return lastContext;
    }
    static BrainfuckContext InternalRun(BrainfuckContext context, CancellationToken cancellationToken)
    {
        var lastContext = context;
        foreach (var command in InternalStepCommands(context))
        {
            lastContext = command.Execute(cancellationToken);
        }
        return lastContext;
    }

    internal static IEnumerable<SequenceCommand> InternalStepCommands(BrainfuckContext context)
    {
        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            var before = context;
            var command2 = new SequenceCommand(command);
            yield return command2;
            if (command2 is not (_, { } executed)) throw new InvalidOperationException($"required {nameof(command2.ExecuteAsync)}() or {nameof(command2.Execute)}() call.");
            context = executed;
        }
    }

    public async ValueTask<string?> RunAndOutputStringAsync(CancellationToken cancellationToken = default)
    {
        var pipe = new Pipe();
        var context = Context with
        {
            Output = pipe.Writer,
        };

        await RunAsync(context, cancellationToken);

        await pipe.Writer.CompleteAsync();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        await using var stream = new MemoryStream();
#else
        using var stream = new MemoryStream();
#endif
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
        await pipe.Reader.CopyToAsync(stream, cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Length == 0) return null;
        var returnString = (await reader.ReadToEndAsync()).TrimEnd('\0');
        if (returnString.Length == 0) return null;
        return returnString;
    }
    public string? RunAndOutputString()
    {
        var pipe = new Pipe();
        var context = Context with
        {
            Output = pipe.Writer,
        };
        Run(context);
        pipe.Writer.Complete();
        if (!pipe.Reader.TryRead(out var result))
            return null;
        var array = result.Buffer.ToArray();
        pipe.Reader.AdvanceTo(result.Buffer.End);
        if (array.Length == 0) return null;
        var returnString = Encoding.UTF8.GetString(array).TrimEnd('\0');
        if (returnString.Length == 0) return null;
        return returnString;
    }

    bool PrintMembers(StringBuilder builder)
    {
        builder.Append(nameof(Sequences) + " = [");
        builder.Append(string.Join(", ", Sequences));
        builder.Append("], " + nameof(Input) + " = ");
        builder.Append(Input);
        builder.Append(", " + nameof(Output) + " = ");
        builder.Append(Output);
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(nameof(BrainfuckProcessor) + " { ");
        if (PrintMembers(builder))
            builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
