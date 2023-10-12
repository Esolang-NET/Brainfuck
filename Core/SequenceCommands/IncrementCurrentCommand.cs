namespace Brainfuck.Core.SequenceCommands;

public class IncrementCurrentCommand : BrainfuckSequenceCommand
{
    public IncrementCurrentCommand(BrainfuckContext context) : base(context) { }
    public override ValueTask<BrainfuckContext> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(IncrementCurrent());
    }
    BrainfuckContext IncrementCurrent()
    {
        var sequencesIndex = context.SequencesIndex + 1;
        var current = context.Stack[context.StackIndex];
        current++;
        var stack = context.Stack.SetItem(context.StackIndex, current);
        return context with
        {
            SequencesIndex = sequencesIndex,
            Stack = stack,
        };
    }
}