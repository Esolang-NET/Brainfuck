using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands;

public class IncrementPointerCommand : BrainfuckSequenceCommand
{
    public IncrementPointerCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IncrementPointer(out var sequencesIndex, out var stack, out var stackIndex);
        return new(new BrainfuckContext(
            Sequences: context.Sequences, SequencesIndex: sequencesIndex,
            Stack: stack, StackIndex: stackIndex,
            Input: context.Input, Output: context.Output
        ));
    }
    void IncrementPointer(out int sequencesIndex, out ImmutableList<byte> stack, out int stackIndex)
    {
        sequencesIndex = context.SequencesIndex + 1;
        stack = sequencesIndex < context.Stack.Count
            ? context.Stack
            : context.Stack.Add(0);
        stackIndex = context.StackIndex + 1;
    }
}