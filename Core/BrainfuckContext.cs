using System.Collections.Immutable;
using System.IO.Pipelines;

namespace Brainfuck;

[Serializable]
public readonly struct BrainfuckContext
{
    public readonly ReadOnlyMemory<BrainfuckSequence> Sequences { get; }
    public readonly int SequencesIndex { get; }
    public readonly ImmutableList<byte> Stack { get; }
    public readonly int StackIndex { get; }
    public readonly PipeReader? Input { get; }
    public readonly PipeWriter? Output { get; }
    public BrainfuckContext(ReadOnlyMemory<BrainfuckSequence> sequences, ImmutableList<byte> stack, int sequencesIndex = default, int stackIndex = default, PipeReader? input = default, PipeWriter? output = default)
        => (Sequences, Stack, SequencesIndex, StackIndex, Input, Output) = (sequences, stack, sequencesIndex, stackIndex, input, output);
    public void Deconstruct(out ReadOnlyMemory<BrainfuckSequence> sequences, out ImmutableList<byte> stack, out int sequencesIndex, out int stackIndex, out PipeReader? input, out PipeWriter? output)
        => (sequences, stack, sequencesIndex, stackIndex, input, output) = (Sequences, Stack, SequencesIndex, StackIndex, Input, Output);
}