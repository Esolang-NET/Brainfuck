namespace Brainfuck.Core.SequenceCommands;

public class DecrementPointerCommand : BrainfuckSequenceCommand
{
    public DecrementPointerCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryDecrementPointer(out var sequencesIndex, out var stackIndex))
            return new(Next());
        return new(context with
        {
            SequencesIndex = sequencesIndex,
            StackIndex = stackIndex
        });
    }
    bool TryDecrementPointer(out int sequencesIndex, out int stackIndex)
    {
        sequencesIndex = default;
        stackIndex = default;
        var sequencesIndex_ = context.SequencesIndex + 1;
        var stackIndex_ = context.StackIndex - 1;
        if (stackIndex_ < 0) return false;
        sequencesIndex = sequencesIndex_;
        stackIndex = stackIndex_;
        return true;
    }

}