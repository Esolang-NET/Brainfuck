namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Executes the <see cref="BrainfuckSequence.IncrementCurrent"/> instruction.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public sealed record IncrementCurrentCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    /// <inheritdoc />
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default) => IncrementCurrent();

    /// <inheritdoc />
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(IncrementCurrent());
    }
    BrainfuckContext IncrementCurrent()
    {
        var sequencesIndex = Context.SequencesIndex + 1;
        var current = Context.Stack[Context.StackIndex];
        current++;
        var stack = Context.Stack.SetItem(Context.StackIndex, current);
        return Context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }
}
