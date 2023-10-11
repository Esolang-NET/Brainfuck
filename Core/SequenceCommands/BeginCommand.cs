using static Brainfuck.BrainfuckSequence;
namespace Brainfuck.Core.SequenceCommands;

public class BeginCommand : BrainfuckSequenceCommand
{
    public BeginCommand(BrainfuckContext context) : base(context) { }

    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sequencesIndex = context.SequencesIndex + 1;
        var current = context.Stack[context.StackIndex];
        if (current is not 0)
            return new(Next());
        var hierarchy = 0;
        var lastIndex = sequencesIndex;
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
            return new(Next());
        sequencesIndex = lastIndex + 1;
        return new(new BrainfuckContext(
            sequences: context.Sequences, sequencesIndex: sequencesIndex,
            stack: context.Stack, stackIndex: context.StackIndex,
            input: context.Input, output: context.Output
        ));
    }
}