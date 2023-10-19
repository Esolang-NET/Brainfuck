namespace Brainfuck.Core.SequenceCommands;

public record EndCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override BrainfuckContext Execute()
    {
        if (!TryGetNextSequencesIndex(out var sequencesIndex))
            return Next();
        return Context with
        {
            SequencesIndex = sequencesIndex,
        };
    }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryGetNextSequencesIndex(out var sequencesIndex))
            return new(Next());
        return new(Context with
        {
            SequencesIndex = sequencesIndex,
        });
    }
    bool TryGetNextSequencesIndex(out int sequencesIndex)
    {
        sequencesIndex = default;
        var current = Context.Stack[Context.StackIndex];
        if (current is 0)
            return false;
        var sequencesIndex_ = Context.SequencesIndex;
        var hierarchy = 0;
        var lastIndex = sequencesIndex_ - 1;
        for (; lastIndex >= 0; lastIndex--)
        {
            var sequence = Context.Sequences.Span[lastIndex];
            if (sequence is not BrainfuckSequence.Begin or BrainfuckSequence.End)
                continue;
            if (sequence is BrainfuckSequence.End)
                hierarchy++;
            else if (hierarchy == 0 && sequence is BrainfuckSequence.Begin)
                break;
            else if (sequence is BrainfuckSequence.Begin)
                hierarchy--;
        }
        if (lastIndex < 0 || Context.Sequences.Span[lastIndex] is not BrainfuckSequence.Begin)
            return false;
        sequencesIndex_ = lastIndex + 1;
        sequencesIndex = sequencesIndex_;
        return true;
    }
}
