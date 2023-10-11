using System.IO.Pipelines;

namespace Brainfuck;

public class BrainfuckRunner
{
    ReadOnlyMemory<BrainfuckSequence> Sequences;
    PipeReader? Input;
    PipeWriter? Output;
    public BrainfuckRunner(string source, BrainfuckOptions? sourceOptions = default, PipeWriter? output = default, PipeReader? input = default)
        : this(SourceToSequences(source, sourceOptions), output: output, input: input) {  }
    private static ReadOnlyMemory<BrainfuckSequence> SourceToSequences(string source, BrainfuckOptions? sourceOptions)
        => new BrainfuckSequencer(source, sourceOptions).Select(v => v.Sequence).ToArray().AsMemory();

    public BrainfuckRunner(ReadOnlyMemory<BrainfuckSequence> sequences, PipeWriter? output = default, PipeReader? input = default)
    {
        Sequences = sequences;
        Input = input;
        Output = output;
    }
    public ValueTask RunAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}