using System.Collections.Immutable;

namespace Brainfuck.Core.SequenceCommands;

public record IncrementPointerCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
    {
        IncrementPointer(out var sequencesIndex, out var stack, out var stackIndex);
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
            StackIndex = stackIndex,
        };
    }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IncrementPointer(out var sequencesIndex, out var stack, out var stackIndex);
        return new(Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
            StackIndex = stackIndex,
        });
    }
    void IncrementPointer(out int sequencesIndex, out ImmutableArray<byte> stack, out int stackIndex)
    {
        sequencesIndex = Context.SequencesIndex + 1;
        stack = sequencesIndex < Context.Stack.Length
            ? Context.Stack
            : Context.Stack.Add(0);
        stackIndex = Context.StackIndex + 1;
    }
}
