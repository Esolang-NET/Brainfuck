using Brainfuck.Core.SequenceCommands;
using System.Collections.Immutable;
using System.IO.Pipelines;

namespace Brainfuck;

public class BrainfuckRunner
{
    readonly ReadOnlyMemory<BrainfuckSequence> Sequences;
    readonly PipeReader? Input;
    readonly PipeWriter? Output;
    public BrainfuckRunner(string source, BrainfuckOptions? sourceOptions = default, PipeWriter? output = default, PipeReader? input = default)
        : this(SourceToSequences(source, sourceOptions), output: output, input: input) { }
    private static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, BrainfuckOptions? sourceOptions)
        => new BrainfuckSequencer(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();

    public BrainfuckRunner(ReadOnlyMemory<BrainfuckSequence> sequences, PipeWriter? output = default, PipeReader? input = default)
    {
        Sequences = sequences;
        Input = input;
        Output = output;
    }
    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        BrainfuckContext context = new(Sequences, sequencesIndex: 0, stack: ImmutableList.Create<byte>(0), stackIndex: 0, input: Input, output: Output);
        while (BrainfuckSequenceCommand.GetCommand(context) is { } command)
        {
            context = await command.ExecuteAsync(cancellationToken);
        }
    }
}