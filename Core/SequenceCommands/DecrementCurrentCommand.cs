using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands;

public record DecrementCurrentCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DecrementCurrent(out var sequencesIndex, out var stack);
        return new(Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        });
    }
    void DecrementCurrent(out int sequencesIndex, out ImmutableList<byte> stack)
    {
        sequencesIndex = Context.SequencesIndex + 1;
        var current = Context.Stack[Context.StackIndex];
        current--;
        stack = Context.Stack.SetItem(Context.StackIndex, current);
    }
}
