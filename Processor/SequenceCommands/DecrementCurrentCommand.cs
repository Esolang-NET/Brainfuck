using System.Collections.Immutable;

namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Executes the <see cref="BrainfuckSequence.DecrementCurrent"/> instruction.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public sealed record DecrementCurrentCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    /// <inheritdoc />
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
    {
        DecrementCurrent(out var sequencesIndex, out var stack);
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }

    /// <inheritdoc />
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
    void DecrementCurrent(out int sequencesIndex, out ImmutableArray<byte> stack)
    {
        sequencesIndex = Context.SequencesIndex + 1;
        var current = Context.Stack[Context.StackIndex];
        current--;
        stack = Context.Stack.SetItem(Context.StackIndex, current);
    }
}
