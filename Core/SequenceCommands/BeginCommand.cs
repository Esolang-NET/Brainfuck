namespace Brainfuck.Core.SequenceCommands;

public class BeginCommand : BrainfuckSequenceCommand
{
    public BeginCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        var current = context.Stack[context.StackIndex];
        if (current is 0)
        {
            var hierarchy = 0;
            var lastIndex = sequencesIndex;
            for (; lastIndex < context.Sequences.Length; lastIndex++)
            {
                var sequence = context.Sequences.Span[lastIndex];
                if (sequence is not BrainfuckSequence.Begin or BrainfuckSequence.End)
                    continue;
                if (sequence is BrainfuckSequence.Begin)
                    hierarchy++;
                else if (hierarchy == 0 && sequence is BrainfuckSequence.End)
                    break;
                else if (sequence is BrainfuckSequence.End)
                    hierarchy--;
            }
            if (context.Sequences.Length > lastIndex && context.Sequences.Span[lastIndex] is BrainfuckSequence.End)
                sequencesIndex = lastIndex + 1;
            else
                throw new InvalidOperationException($"not found {BrainfuckSequence.End}");
        }
        return new(new BrainfuckContext(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: context.Stack, stackIndex: context.StackIndex,
            input: context.Input, output: context.Output
        ));
    }
}