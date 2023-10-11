namespace Brainfuck.Core.SequenceCommands;

public class EndCommand : BrainfuckSequenceCommand
{
    public EndCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var current = context.Stack[context.StackIndex];
        if (current is 0)
            return new(Next());
        var sequencesIndex = context.SequencesIndex;
        var hierarchy = 0;
        var lastIndex = sequencesIndex - 1;
        for (; lastIndex >= 0; lastIndex--)
        {
            var sequence = context.Sequences.Span[lastIndex];
            if (sequence is not BrainfuckSequence.Begin or BrainfuckSequence.End)
                continue;
            if (sequence is BrainfuckSequence.End)
                hierarchy++;
            else if (hierarchy == 0 && sequence is BrainfuckSequence.Begin)
                break;
            else if (sequence is BrainfuckSequence.Begin)
                hierarchy--;
        }
        if (lastIndex < 0 || context.Sequences.Span[lastIndex] is not BrainfuckSequence.Begin)
            return new(Next());
        sequencesIndex = lastIndex + 1;
        return new(new BrainfuckContext(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: context.Stack, stackIndex: context.StackIndex,
            input: context.Input, output: context.Output
        ));

    }
}