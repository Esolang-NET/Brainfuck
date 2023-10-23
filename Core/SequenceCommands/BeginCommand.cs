using static Brainfuck.BrainfuckSequence;
namespace Brainfuck.Core.SequenceCommands;

public record BeginCommand(BrainfuckContext Context) : BrainfuckSequenceCommand(Context)
{
    public override BrainfuckContext Execute(CancellationToken cancellationToken = default)
    {
        if (!TryBegin(out var sequencesIndex))
            return Next();
        return Context with
        {
            SequencesIndex = sequencesIndex,
        };
    }
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryBegin(out var sequencesIndex))
            return new(Next());
        return new(Context with
        {
            SequencesIndex = sequencesIndex,
        });
    }
    bool TryBegin(out int sequencesIndex)
    {
        sequencesIndex = default;
        var sequencesIndex_ = Context.SequencesIndex + 1;
        var current = Context.Stack[Context.StackIndex];
        if (current is not 0)
            return false;
        var hierarchy = 0;
        var lastIndex = sequencesIndex_;
        for (; lastIndex < Context.Sequences.Length; lastIndex++)
        {
            var sequence = Context.Sequences.Span[lastIndex];
            if (sequence is not (Begin or End))
                continue;
            if (sequence is Begin)
                hierarchy++;
            else if (hierarchy == 0 && sequence is End)
                break;
            else if (sequence is End)
                hierarchy--;
        }
        if (Context.Sequences.Length <= lastIndex || Context.Sequences.Span[lastIndex] is not End)
            return false;
        sequencesIndex_ = lastIndex + 1;
        sequencesIndex = sequencesIndex_;
        return true;

    }
}
