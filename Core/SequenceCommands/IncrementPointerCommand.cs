using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands;

public record IncrementPointerCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IncrementPointer(out var sequencesIndex, out var stack, out var stackIndex);
        return new(new BrainfuckContext(
            Sequences: Context.Sequences, SequencesIndex: sequencesIndex,
            Stack: stack, StackIndex: stackIndex,
            Input: Context.Input, Output: Context.Output
        ));
    }
    void IncrementPointer(out int sequencesIndex, out ImmutableList<byte> stack, out int stackIndex)
    {
        sequencesIndex = Context.SequencesIndex + 1;
        stack = sequencesIndex < Context.Stack.Count
            ? Context.Stack
            : Context.Stack.Add(0);
        stackIndex = Context.StackIndex + 1;
    }
}
