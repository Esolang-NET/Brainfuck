﻿using Brainfuck.Core.SequenceCommands;
using System.Collections.Immutable;
using System.IO.Pipelines;
using System.Text;

namespace Brainfuck;

/// <summary>
/// 
/// </summary>
public sealed partial class BrainfuckRunner
{
    readonly ReadOnlyMemory<BrainfuckSequence> Sequences;
    readonly PipeReader? Input;
    readonly PipeWriter? Output;
    BrainfuckContext Context => new(Sequences, SequencesIndex: 0, Stack: ImmutableList.Create<byte>(0), StackIndex: 0, Input: Input, Output: Output);
    public BrainfuckRunner(string source, BrainfuckOptions? sourceOptions = default, PipeWriter? output = default, PipeReader? input = default)
        : this(SourceToSequences(source, sourceOptions), output: output, input: input) { }
    static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, BrainfuckOptions? sourceOptions)
        => new BrainfuckSequenceEnumerable(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();

    public BrainfuckRunner(ReadOnlyMemory<BrainfuckSequence> sequences, PipeWriter? output = default, PipeReader? input = default)
        => (Sequences, Input, Output) = (sequences, input, output);

    public void Deconstruct(out ReadOnlyMemory<BrainfuckSequence> sequences, out PipeWriter? output, out PipeReader? input)
        => (sequences, input, output) = (Sequences, Input, Output);

    public ValueTask<BrainfuckContext> RunAsync(CancellationToken cancellationToken = default) => RunAsync(Context, cancellationToken);
    public IEnumerable<BrainfuckSequenceCommand> RunStep() => InternalRunStep(Context);
    static async ValueTask<BrainfuckContext> RunAsync(BrainfuckContext context, CancellationToken cancellationToken = default)
    {
        var lastContext = context;
        foreach (var command in InternalRunStep(context))
        {
            lastContext = await command.ExecuteAsync(cancellationToken);
        }
        return lastContext;
    }

    internal static IEnumerable<SequenceCommand> InternalRunStep(BrainfuckContext context)
    {
        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            var before = context;
            var command2 = new SequenceCommand(command);
            yield return command2;
            if (command2.Executed is null) throw new InvalidOperationException($"required {nameof(command2.ExecuteAsync)}() call.");
            context = command2.Executed.Value;
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
        using var stream = new MemoryStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
        await pipe.Reader.CopyToAsync(stream, cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Length == 0) return null;
        return await reader.ReadToEndAsync();

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
        builder.Append(nameof(BrainfuckRunner) + " { ");
        if (PrintMembers(builder))
            builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
