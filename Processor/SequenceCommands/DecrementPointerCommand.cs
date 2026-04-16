namespace Esolang.Brainfuck.Processor.SequenceCommands;

/// <summary>
/// Executes the <see cref="BrainfuckSequence.DecrementPointer"/> instruction.
/// </summary>
/// <param name="Context">The context to execute against.</param>
public sealed record DecrementPointerCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    /// <inheritdoc />
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
    {
        if (!TryDecrementPointer(out var sequencesIndex, out var stackIndex))
            return Next();
        return Context with
        {
            SequencesIndex = sequencesIndex,
            StackIndex = stackIndex,
        };
    }

    /// <inheritdoc />
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryDecrementPointer(out var sequencesIndex, out var stackIndex))
            return new(Next());
        return new(Context with
        {
            SequencesIndex = sequencesIndex,
            StackIndex = stackIndex,
        });
    }
    bool TryDecrementPointer(out int sequencesIndex, out int stackIndex)
    {
        sequencesIndex = default;
        stackIndex = default;
        var sequencesIndex_ = Context.SequencesIndex + 1;
        var stackIndex_ = Context.StackIndex - 1;
        if (stackIndex_ < 0) return false;
        sequencesIndex = sequencesIndex_;
        stackIndex = stackIndex_;
        return true;
    }

}
