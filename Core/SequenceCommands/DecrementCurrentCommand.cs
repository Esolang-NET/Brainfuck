using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands;

public class DecrementCurrentCommand : BrainfuckSequenceCommand
{
    public DecrementCurrentCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DecrementCurrent(out var sequencesIndex, out var stack);
        return new(context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        });
    }
    void DecrementCurrent(out int sequencesIndex, out ImmutableList<byte> stack)
    {
        sequencesIndex = context.SequencesIndex + 1;
        var current = context.Stack[context.StackIndex];
        current--;
        stack = context.Stack.SetItem(context.StackIndex, current);
    }
}