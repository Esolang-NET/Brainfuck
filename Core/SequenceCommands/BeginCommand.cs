using static Brainfuck.BrainfuckSequence;
namespace Brainfuck.Core.SequenceCommands;

public class BeginCommand : BrainfuckSequenceCommand
{
    public BeginCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryBegin(out var sequencesIndex))
            return new(Next());
        return new(context with
        {
            SequencesIndex = sequencesIndex,
        });
    }
    bool TryBegin(out int sequencesIndex)
    {
        sequencesIndex = default;
        var sequencesIndex_ = context.SequencesIndex + 1;
        var current = context.Stack[context.StackIndex];
        if (current is not 0)
            return false;
        var hierarchy = 0;
        var lastIndex = sequencesIndex_;
        for (; lastIndex < context.Sequences.Length; lastIndex++)
        {
            var sequence = context.Sequences.Span[lastIndex];
            if (sequence is not (Begin or End))
                continue;
            if (sequence is Begin)
                hierarchy++;
            else if (hierarchy == 0 && sequence is End)
                break;
            else if (sequence is End)
                hierarchy--;
        }
        if (context.Sequences.Length <= lastIndex || context.Sequences.Span[lastIndex] is not End)
            return false;
        sequencesIndex_ = lastIndex + 1;
        sequencesIndex = sequencesIndex_;
        return true;

    }
}